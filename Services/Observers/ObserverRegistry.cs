using Core.Interfaces;

namespace Services.Observers
{
    /// <summary>
    /// Singleton-реестр наблюдателей. В системе существует ровно один экземпляр,
    /// управляющий подпиской на события всех юнитов.
    /// </summary>
    public sealed class ObserverRegistry
    {
        private static readonly ObserverRegistry _instance = new();
        public static ObserverRegistry Instance => _instance;
        private ObserverRegistry() { }

        // Публичные свойства для доступа к специфичным методам (ClearLog, IsEnabled)
        public UnitDeathObserver DeathObserver { get; } = new();
        public UnitHealthObserver HealthObserver { get; } = new();

        // Все наблюдатели через интерфейс 
        private IReadOnlyList<IUnitObserver> Observers =>
            new List<IUnitObserver> { DeathObserver, HealthObserver };

        public void Attach(IUnit unit)
        {
            foreach (var observer in Observers)
            {
                observer.Unsubscribe(unit);
                observer.Subscribe(unit);
            }
        }

        public void Attach(IArmy army)
        {
            foreach (var unit in army.Units)
                Attach(unit);
        }

        public void Detach(IUnit unit)
        {
            foreach (var observer in Observers)
                observer.Unsubscribe(unit);
        }

        public void Detach(IArmy army)
        {
            foreach (var unit in army.Units)
                Detach(unit);
        }
    }
}