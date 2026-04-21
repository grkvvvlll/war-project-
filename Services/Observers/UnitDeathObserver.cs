using System.Text;
using Core.Interfaces;

namespace Services.Observers
{
    public class UnitDeathObserver
    {
        private static readonly object _lock = new();

        private readonly string _logDir =
            Path.Combine(AppContext.BaseDirectory, "logs");

        private readonly string _logPath =
            Path.Combine(AppContext.BaseDirectory, "logs", "damage-log.txt");

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

            Directory.CreateDirectory(_logDir);

            string deathLine = $"[BEEP] {unit.Name} погиб";

            lock (_lock)
            {
                File.AppendAllText(_logPath, deathLine + Environment.NewLine, Encoding.UTF8);
            }

            Console.WriteLine($"\a🔔 {unit.Name} погиб!");

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