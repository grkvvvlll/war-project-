using Core.Interfaces;
using Core.Entities.Units;
using Core.Entities.Buffs;

namespace Core.Entities.Abilities
{
    public class SquireAbility : ISpecialAbility
    {
        private readonly IRandomService _random;
        private readonly int _chanceToApply;

        public string Name => "Оруженосец";
        public string Description => $"Может надеть уникальный бафф на соседнего Heavy юнита с шансом {_chanceToApply}%";

        public SquireAbility(IRandomService random, int chanceToApply = 70)
        {
            _random = random;
            _chanceToApply = chanceToApply;
        }

        public bool CanUse(IUnit user)
        {
            return user.IsAlive;
        }

        public bool CanTarget(IUnit user, IUnit target, bool isAlly)
        {
            if (!isAlly || !target.IsAlive) return false;

            // Проверяем, является ли цель HeavyUnit 
            if (!IsHeavyUnit(target)) return false;

            int currentBuffCount = GetBuffCount(target);
            return currentBuffCount < 4;
        }

        private bool IsHeavyUnit(IUnit unit)
        {
            while (unit is UnitDecorator decorator)
            {
                unit = decorator.GetInnerUnit();
            }
            return unit is HeavyUnit;
        }

        private int GetBuffCount(IUnit unit)
        {
            int count = 0;
            while (unit is UnitDecorator decorator)
            {
                count++;
                unit = decorator.GetInnerUnit();
            }
            return count;
        }

        public void Use(IUnit user, IUnit target, int distance)
        {
            int roll = _random.Next(0, 100);
            if (roll >= _chanceToApply)
            {
                this.LastAppliedUnit = null; // Неудача
                return;
            }

            // Получаем список уже имеющихся баффов
            HashSet<string> existingBuffs = new HashSet<string>();
            if (target is UnitDecorator targetDecorator)
            {
                existingBuffs = targetDecorator.GetAllBuffTypes();
            }

            // Список возможных декораторов
            var potentialBuffs = new List<(string Type, Func<IUnit, IUnit> Creator)>
            {
                ("Horse", u => new HorseDecorator(u)),
                ("Spear", u => new SpearDecorator(u)),
                ("Shield", u => new ShieldDecorator(u)),
                ("Helmet", u => new HelmetDecorator(u))
            };

            // Фильтруем те, которых еще нет
            var availableBuffs = potentialBuffs.Where(b => !existingBuffs.Contains(b.Type)).ToList();

            if (!availableBuffs.Any())
            {
                // Все возможные баффы уже надеты 
                this.LastAppliedUnit = null;
                return;
            }

            // Выбираем случайный из доступных
            var chosen = availableBuffs[_random.Next(0, availableBuffs.Count)];

            var newBuffedUnit = chosen.Creator(target);

            this.LastAppliedUnit = newBuffedUnit;
        }

        public IUnit? LastAppliedUnit { get; private set; }

        public void ResetCharge() { }
        public void Charge() { }

        public int Execute(IUnit user, int userIndex,
                           IArmy ownArmy, IArmy enemyArmy,
                           bool isArmy1, IAbilityExecutionContext ctx)
        {
            var neighborIndices = ctx.GetNeighborIndices(ownArmy, userIndex, isArmy1, maxDist: 1);

            IUnit? targetHeavy = null;
            int targetIndex = -1;

            foreach (var idx in neighborIndices)
            {
                var neighbor = ownArmy.Units[idx];
                if (!neighbor.IsAlive) continue;
                if (!CanTarget(user, neighbor, isAlly: true)) continue;
                targetHeavy = neighbor;
                targetIndex = idx;
                break;
            }

            if (targetHeavy == null) return 0;

            int oldAttack = targetHeavy.Attack;
            int oldDefence = targetHeavy.Defence;

            Use(user, targetHeavy, distance: 0);

            if (LastAppliedUnit == null) return 0;

            ownArmy.SetUnit(targetIndex, LastAppliedUnit);

            string buffName = "Бафф";
            int atkDelta = 0, defDelta = 0;

            if (LastAppliedUnit is UnitDecorator dec)
            {
                var buff = dec.GetCurrentBuff();
                buffName = buff.NameNominative;
                atkDelta = buff.AttackBonus;
                defDelta = buff.DefenceBonus;
            }

            if (atkDelta != 0 || defDelta != 0)
            {
                var stats = new List<string>();
                if (atkDelta != 0) stats.Add($"ATK {oldAttack} -> {oldAttack + atkDelta}");
                if (defDelta != 0) stats.Add($"DEF {oldDefence} -> {oldDefence + defDelta}");
                ctx.Logger.Log($"   Характеристики: {string.Join(", ", stats)}");
            }

            ctx.Logger.LogBuffAdded(user, targetHeavy, buffName, isArmy1);
            return 0;
        }
    }
}