using System;
using System.Collections.Generic;
using System.Linq;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Entities
{
    public class Army : IArmy
    {
        private readonly List<IUnit> _units;

        // Индекс текущего активного юнита
        private int _currentIndex = 0;

        public string Name { get; }

        public IReadOnlyList<IUnit> Units => _units;

        // Есть ли в армии живые юниты.
        public bool HasAliveUnits => _units.Any(u => u.IsAlive);

        // Общая стоимость армии.
        public int TotalCost => _units.Sum(u => u.Cost);

        public Army(string name, IEnumerable<IUnit> units)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            _units = units?.ToList()
                ?? throw new ArgumentNullException(nameof(units));

            if (!_units.Any())
                throw new ArgumentException("Army must contain at least one unit.");
        }

        // Возвращаем текущего живого юнита. Если текущий мёртв — ищет следующего живого
        public IUnit GetCurrentAliveUnit()
        {
            if (!HasAliveUnits)
                throw new InvalidOperationException("No alive units in the army.");

            while (_currentIndex < _units.Count && !_units[_currentIndex].IsAlive)
            {
                _currentIndex++;
            }

            return _units[_currentIndex];
        }

        // Переключает армию на следующего живого юнита
        public void MoveToNextAliveUnit()
        {
            _currentIndex++;

            while (_currentIndex < _units.Count && !_units[_currentIndex].IsAlive)
            {
                _currentIndex++;
            }
        }
    }
}