using System.Text;
using Core.Interfaces;

namespace Core.Entities.Units.Proxies
{
    public static class UnitProxySupport
    {
        private static readonly object _lock = new();

        private static readonly string _logDir =
            Path.Combine(AppContext.BaseDirectory, "logs");

        private static readonly string _logPath =
            Path.Combine(_logDir, "damage-log.txt");

        public static void AfterDamage(IUnit unit, int damage, int oldHp, bool wasAlive)
        {
            Directory.CreateDirectory(_logDir);

            string line =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {unit.Name} получил {damage} урона | HP: {oldHp} -> {unit.Health}";

            lock (_lock)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
            }

            if (wasAlive && !unit.IsAlive)
            {
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
}