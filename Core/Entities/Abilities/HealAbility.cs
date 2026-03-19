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
    }
}