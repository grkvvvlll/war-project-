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

        public event Action<IUnit, IUnit>? CloneCreated;

        public string Name => "Клонирование";
        public string Description => $"С вероятностью {_baseChance}% создаёт клона союзника";

        public CloneAbility(int range, int baseChance, IRandomService random)
        {
            _range = range;
            _baseChance = baseChance;
            _currentChance = baseChance;
            _random = random;
        }

        public bool CanUse(IUnit user) => user.IsAlive;

        public bool CanTarget(IUnit user, IUnit target, bool isAlly)
        {
            if (!isAlly || !target.IsAlive)
                return false;
            return target is LightUnit || target is Archer;
        }

        public void Use(IUnit user, IUnit target, int distance)
        {
            if (_range < distance)
            {
                _currentChance = Math.Min(100, _currentChance + 5);
                return;
            }

            int roll = _random.Next(0, 100);

            if (roll >= _currentChance)
            {
                _currentChance = Math.Min(100, _currentChance + 5);
                return;
            }

            if (target is ICanBeCloned cloneable)
            {
                var clone = cloneable.Clone(_random);
                CloneTargetName = target.Name;
                CloneCreated?.Invoke(user, clone);
            }

            ResetCharge();
        }

        public string? CloneTargetName { get; private set; }

        public void ResetCharge() => _currentChance = _baseChance;
        public void Charge() => _currentChance = Math.Min(100, _currentChance + 5);

        //  Метод для получения текущей вероятности
        public int GetCurrentChance() => _currentChance;
        public int GetBaseChance() => _baseChance;
    }
}