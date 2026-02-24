using System.Collections.Generic;

namespace gaaameee.Core.Interfaces
{
    public interface IArmy
    {
        string Name { get; }
        IReadOnlyList<IUnit> Units { get; }

        bool HasAliveUnits { get; }

        IUnit GetCurrentAliveUnit();
        void MoveToNextAliveUnit();

        int TotalCost { get; }
    }
}