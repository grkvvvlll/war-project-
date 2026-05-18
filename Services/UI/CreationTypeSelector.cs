namespace Services.UI
{
    public class CreationTypeSelector
    {
        public bool Select(string armyName)
        {
            while (true)
            {
                Console.WriteLine($"\n{armyName}:");
                Console.WriteLine("1. Автоматическое создание");
                Console.WriteLine("2. Ручное создание");
                Console.Write("Выберите способ (1-2): ");

                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                    return true;
                if (choice == "2")
                    return false;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Неверный ввод. Пожалуйста, введите 1 или 2.");
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }
}