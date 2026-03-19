using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Factories.Armies
{
    public interface IArmyFactory
    {
        string FactoryName { get; }
        IArmy CreateArmy(string name, int budget);
    }
}