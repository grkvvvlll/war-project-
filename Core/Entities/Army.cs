using Core.Interfaces;

namespace Core.Entities
{
    public class Army : IArmy
    {
        private readonly List<IUnit> _units;

        public string Name { get; }
        public IReadOnlyList<IUnit> Units => _units;

        public bool HasAliveUnits => _units.Any(u => u.IsAlive);

        public int TotalCost => _units.Sum(u => u.Cost);

        public Army(string name, IEnumerable<IUnit> units)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _units = units?.ToList() ?? throw new ArgumentNullException(nameof(units));
            if (!_units.Any())
                throw new ArgumentException("Army must contain хотя бы одного юнита.");
        }

        // Core/Entities/Army.cs
        public void InsertUnit(IUnit unit, int position)
        {
            if (position < 0)
                position = 0;
            if (position > _units.Count)
                position = _units.Count;

            _units.Insert(position, unit);
        }

        // Просто возвращает текущего живого юнита (для последовательного обхода)
        private int _currentIndex = 0;
        public IUnit GetCurrentAliveUnit()
        {
            while (_currentIndex < _units.Count && !_units[_currentIndex].IsAlive)
            {
                _currentIndex++;
            }

            if (_currentIndex >= _units.Count)
                throw new InvalidOperationException("Нет живых юнитов.");

            return _units[_currentIndex];
        }

        public void MoveToNextAliveUnit()
        {
            _currentIndex++;
            while (_currentIndex < _units.Count && !_units[_currentIndex].IsAlive)
            {
                _currentIndex++;
            }
        }

        public void RemoveFrontUnit()
        {
            if (_units.Count > 0)
                _units.RemoveAt(0);
        }

        public IUnit GetFrontUnit()
        {
            return _units.First(u => u.IsAlive);
        }

        public void RemoveDeadUnits()
        {
            _units.RemoveAll(u => !u.IsAlive);
        }

        public void SetUnit(int index, IUnit unit)
        {
            if (index >= 0 && index < _units.Count)
            {
                _units[index] = unit;
            }
        }
    }
}