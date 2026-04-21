using Core.Entities;
using Core.Interfaces;

namespace Services.Observers
{
    public static class ObserverRegistry
    {
        public static UnitDeathObserver DeathObserver { get; } = new();
        public static UnitHealthObserver HealthObserver { get; } = new();

        public static void Attach(IUnit unit)
        {
            DeathObserver.Unsubscribe(unit);
            HealthObserver.Unsubscribe(unit);

            DeathObserver.Subscribe(unit);
            HealthObserver.Subscribe(unit);
        }

        public static void Attach(IArmy army)
        {
            foreach (var unit in army.Units)
                Attach(unit);
        }

        public static void Detach(IUnit unit)
        {
            DeathObserver.Unsubscribe(unit);
            HealthObserver.Unsubscribe(unit);
        }

        public static void Detach(IArmy army)
        {
            foreach (var unit in army.Units)
                Detach(unit);
        }
    }
}