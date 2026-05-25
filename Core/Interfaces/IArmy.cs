namespace Core.Interfaces
{
    public interface IArmy
    {
        string Name { get; }
        IReadOnlyList<IUnit> Units { get; }
        bool HasAliveUnits { get; }
        int TotalCost { get; }
        IUnit GetFrontUnit();
        void RemoveFrontUnit();
        void RemoveDeadUnits();
        void InsertUnit(IUnit unit, int position);
        void SetUnit(int index, IUnit unit);
        void ReverseUnits();
    }
}