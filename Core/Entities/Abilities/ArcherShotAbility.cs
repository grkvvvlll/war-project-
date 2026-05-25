using Core.Interfaces;

namespace Core.Entities.Abilities
{
    public class ArcherShotAbility : ISpecialAbility
    {
        private readonly int _range;

        public string Name => "Выстрел стрелой";
        public string Description => $"Атакует врага на дистанции до {_range}";

        public ArcherShotAbility(int range)
        {
            _range = range;
        }

        public bool CanUse(IUnit user)
        {
            return user.IsAlive;
        }

        public bool CanTarget(IUnit user, IUnit target, bool isAlly)
        {
            return !isAlly && target.IsAlive;
        }

        public void Use(IUnit user, IUnit target, int distance)
        {
            if (!target.IsAlive)
                return;

            if (_range < distance)
                return;

            int damage = user.Attack - target.Defence;
            if (damage < 0)
                damage = 0;

            target.TakeDamage(damage);
        }

        public void ResetCharge() { }
        public void Charge() { }

        public int Execute(IUnit user, int userIndex,
                           IArmy ownArmy, IArmy enemyArmy,
                           bool isArmy1, IAbilityExecutionContext ctx)
        {
            // Найти врага в радиусе; если нет — взять ближайшего 
            var validInRange = new List<(IUnit unit, int dist)>();
            IUnit? fallback = null;
            int fallbackDist = 999;

            for (int j = 0; j < enemyArmy.Units.Count; j++)
            {
                var enemy = enemyArmy.Units[j];
                if (!enemy.IsAlive) continue;
                int dist = ctx.GetEnemyDistance(ownArmy, userIndex, enemyArmy, j, isArmy1);
                if (dist <= _range)
                    validInRange.Add((enemy, dist));
                if (dist < fallbackDist) { fallback = enemy; fallbackDist = dist; }
            }

            IUnit? target;
            int targetDist;

            if (validInRange.Any())
            {
                (target, targetDist) = validInRange[ctx.Random.Next(0, validInRange.Count)];
            }
            else if (fallback != null)
            {
                target = fallback;
                targetDist = fallbackDist;
            }
            else return 0;

            int oldHp = target.Health;
            Use(user, target, targetDist);

            ctx.Logger.LogArcherShot(user, _range, targetDist, isArmy1);
            if (_range < targetDist)
                ctx.Logger.LogArrowMiss();
            else
                ctx.Logger.LogArcherHit(user, target, oldHp, target.Health, isArmy1);

            if (!target.IsAlive && oldHp > 0)
            {
                ctx.Logger.LogDeath(target, !isArmy1);
                return target.Cost;
            }
            return 0;
        }
    }
}