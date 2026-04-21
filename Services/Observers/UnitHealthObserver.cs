using System.Text;
using Core.Interfaces;

namespace Services.Observers
{
    public class UnitHealthObserver
    {
        private static readonly object _lock = new();

        private readonly string _logDir =
            Path.Combine(AppContext.BaseDirectory, "logs");

        private readonly string _logPath =
            Path.Combine(AppContext.BaseDirectory, "logs", "damage-log.txt");

        public bool IsEnabled { get; set; } = true;

        public void Subscribe(IUnit unit)
        {
            unit.HealthChanged -= OnHealthChanged;
            unit.HealthChanged += OnHealthChanged;
        }

        public void Unsubscribe(IUnit unit)
        {
            unit.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(IUnit unit, int oldHp, int newHp)
        {
            if (!IsEnabled)
                return;

            Directory.CreateDirectory(_logDir);

            string line =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {unit.Name} | HP: {oldHp} -> {newHp}";

            lock (_lock)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}