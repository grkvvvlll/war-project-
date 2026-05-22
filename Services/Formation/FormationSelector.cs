using Core.Formations;
using Core.Interfaces;

namespace Services.Formation
{
    public class FormationSelector
    {
        public IBattleFormation Select()
        {
            while (true)
            {
                Console.WriteLine("Выберите способ построения:");
                Console.WriteLine("1. Бой на мосту");
                Console.WriteLine("2. Бой на широком мосту");
                Console.WriteLine("3. Стенка на стенку");
                Console.Write("Выберите построение (1-3): ");
                var input = Console.ReadLine()?.Trim();

                if (input == "1") return new BridgeFormation();
                if (input == "2") return new WideBridgeFormation();
                if (input == "3") return new WallFormation();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Неверный ввод. Пожалуйста, введите 1, 2 или 3.");
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }
}