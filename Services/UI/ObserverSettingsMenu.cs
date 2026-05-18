using Services.Observers;

namespace Services.UI
{
    public class ObserverSettingsMenu
    {
        public void Show()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Настройки наблюдателей ===");
                Console.WriteLine($"1. Звук при смерти юнита: {(ObserverRegistry.DeathObserver.IsEnabled ? "вкл" : "выкл")}");
                Console.WriteLine($"2. Файловый лог изменений HP: {(ObserverRegistry.HealthObserver.IsEnabled ? "вкл" : "выкл")}");
                Console.WriteLine();
                Console.WriteLine("Наблюдатель 2 пишет только в logs/damage-log.txt.");
                Console.WriteLine("Если он выключен, изменения HP в файл не добавляются.");
                Console.WriteLine("Наблюдатель 1 только подаёт звук при смерти юнита.");
                Console.WriteLine("Боевые сообщения в консоли выводит обычный логгер боя.");
                Console.WriteLine("0. Назад");
                Console.Write("Выберите пункт: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1":
                        ObserverRegistry.DeathObserver.IsEnabled = !ObserverRegistry.DeathObserver.IsEnabled;
                        break;
                    case "2":
                        ObserverRegistry.HealthObserver.IsEnabled = !ObserverRegistry.HealthObserver.IsEnabled;
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}