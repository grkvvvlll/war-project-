using System;
using Core.Interfaces;
using Core.Entities.Units;

namespace Core.Entities.Abilities
{
    public class CloneAbility : ISpecialAbility
    {
        private readonly int _range;
        private readonly int _baseChance;
        private int _currentChance;
        private readonly IRandomService _random;

        public string Name => "Клонирование";
        public string Description => $"С вероятностью {_baseChance}% создаёт клона союзника";

        public CloneAbility(int range, int baseChance, IRandomService random)
        {
            _range = range;
            _baseChance = baseChance;
            _currentChance = baseChance;
            _random = random;
        }

        public bool CanUse(IUnit user)
        {
            return user.IsAlive;
        }

        public bool CanTarget(IUnit user, IUnit target, bool isAlly)
        {
            if (!isAlly || !target.IsAlive)
                return false;

            return target is LightUnit || target is Archer;
        }

        public void Use(IUnit user, IUnit target, int distance)
        {
            if (_range < distance)
                return;

            int roll = _random.Next(0, 100);
            if (roll >= _currentChance)
            {
                _currentChance = Math.Min(100, _currentChance + 10);
                return;
            }

            if (target is ICanBeCloned cloneable)
            {
                var clone = cloneable.Clone(_random);
                Console.WriteLine($"✨ {user.Name} создал клона {target.Name}!");
            }

            ResetCharge();
        }

        public void ResetCharge()
        {
            _currentChance = _baseChance;
        }

        public void Charge()
        {
            _currentChance = Math.Min(100, _currentChance + 10);
        }
    }
}