using Core.Interfaces;
using Core.Entities.Units;

namespace Core.Entities.Abilities
{
    public class HealAbility : ISpecialAbility
    {
        private readonly int _range;
        private readonly int _healPower;

        public string Name => "Лечение";
        public string Description => $"Восстанавливает {_healPower} HP союзнику в радиусе {_range}";

        public HealAbility(int range, int healPower)
        {
            _range = range;
            _healPower = healPower;
        }

        public bool CanUse(IUnit user)
        {
            return user.IsAlive;
        }

        public bool CanTarget(IUnit user, IUnit target, bool isAlly)
        {
            if (!isAlly || !target.IsAlive)
                return false;

            if (target == user)
                return false;

            if (target is HeavyUnit)
                return false;

            return target is ICanBeHealed || target is Healer;
        }

        public void Use(IUnit user, IUnit target, int distance)
        {
            if (_range < distance)
                return;

            target.Heal(_healPower);
        }

        public void ResetCharge() { }
        public void Charge() { }

        public int Execute(IUnit user, int userIndex,
                           IArmy ownArmy, IArmy enemyArmy,
                           bool isArmy1, IAbilityExecutionContext ctx)
        {
            // Найти союзника в радиусе лечения
            var validTargets = new List<IUnit>();

            for (int j = 0; j < ownArmy.Units.Count; j++)
            {
                var ally = ownArmy.Units[j];
                if (!ally.IsAlive || ally == user) continue;
                if (!CanTarget(user, ally, isAlly: true)) continue;
                int dist = ctx.GetAllyDistance(ownArmy, userIndex, j, isArmy1);
                if (dist <= _range)
                    validTargets.Add(ally);
            }

            if (!validTargets.Any()) return 0;

            var target = validTargets[ctx.Random.Next(0, validTargets.Count)];
            int oldHp = target.Health;
            Use(user, target, distance: 0);

            int healed = target.Health - oldHp;
            if (healed > 0)
                ctx.Logger.LogHeal(user, target, healed, isArmy1);
            else
                ctx.Logger.LogHealNoEffect(user, target, isArmy1);

            return 0;
        }
    }
}