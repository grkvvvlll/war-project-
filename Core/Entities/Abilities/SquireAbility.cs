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

            // Проверяем, является ли цель HeavyUnit (или декоратором вокруг него)
            if (!IsHeavyUnit(target)) return false;

            // Проверяем доступные слоты и дубликаты
            // Мы не знаем заранее, какой бафф выберем, поэтому проверка "можно ли надеть ХОТЯ БЫ ОДИН бафф"
            // делается здесь. Конкретная проверка на дубликат конкретного баффа будет при использовании (Use),
            // так как там мы выбираем случайный бафф.

            // Но для CanTarget достаточно знать, что есть место ИЛИ есть уникальные баффы, которые можно надеть.
            // Упростим: если на юните меньше 4 баффов, теоретически можно надеть что-то новое.
            // Точная проверка типа баффа будет внутри Use.

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
                // Все возможные уникальные баффы уже надеты (даже если слотов < 4, но типов всего 4)
                this.LastAppliedUnit = null;
                return;
            }

            // Выбираем случайный из доступных
            var chosen = availableBuffs[_random.Next(0, availableBuffs.Count)];

            // Создаем новый декорированный юнит
            var newBuffedUnit = chosen.Creator(target);

            this.LastAppliedUnit = newBuffedUnit;
        }

        public IUnit? LastAppliedUnit { get; private set; }

        public void ResetCharge() { }
        public void Charge() { }
    }
}