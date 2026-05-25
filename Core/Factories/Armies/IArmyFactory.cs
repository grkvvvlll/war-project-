using Core.Interfaces;

namespace Core.Factories.Armies
{
    // абстракт
    public interface IArmyFactory
    {
        string FactoryName { get; }

        // Подготовить фабрику к созданию армии.
        void PrepareCreation(IEnumerable<string>? choices = null);

        IArmy CreateArmy(string name, int budget);
    }
}