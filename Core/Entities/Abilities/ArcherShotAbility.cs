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
    }
}