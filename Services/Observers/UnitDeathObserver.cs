using Core.Interfaces;

namespace Services.Observers
{
    public class UnitDeathObserver
    {
        public bool IsEnabled { get; set; } = true;

        public void Subscribe(IUnit unit)
        {
            unit.Died -= OnUnitDied;
            unit.Died += OnUnitDied;
        }

        public void Unsubscribe(IUnit unit)
        {
            unit.Died -= OnUnitDied;
        }

        private void OnUnitDied(IUnit unit)
        {
            if (!IsEnabled)
                return;

            if (!OperatingSystem.IsWindows())
                return;

            try
            {
                Console.Beep(1200, 300);
            }
            catch
            {
            }
        }
    }
}
