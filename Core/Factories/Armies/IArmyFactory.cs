using Core.Interfaces;

namespace Core.Factories.Armies
{
    // абстракт
    public interface IArmyFactory
    {
        string FactoryName { get; }
        IArmy CreateArmy(string name, int budget);
    }
}