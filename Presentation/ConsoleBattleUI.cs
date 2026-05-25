using Core.Formations;
using Core.Interfaces;

namespace Presentation
{
    // Консольная реализация IBattleUI
    public class ConsoleBattleUI : IBattleUI
    {
        public RoundMenuChoice WaitForChoice(int roundNumber)
        {
            Console.WriteLine();
            Console.WriteLine($"МЕНЮ (перед {roundNumber}-м раундом)");
            Console.WriteLine("Enter - следующий раунд");
            Console.WriteLine("1 - показать состав армий");
            Console.WriteLine("2 - сохранить и выйти в меню");
            Console.WriteLine("3 - проиграть до конца");
            Console.WriteLine("4 - выйти в меню без сохранения");
            Console.WriteLine("5 - изменить построение армий");
            Console.WriteLine("6 - Undo");
            Console.WriteLine("7 - Redo");
            Console.WriteLine("8 - сброс в исходное состояние");
            Console.WriteLine("9 - показать историю действий");
            Console.Write("Ваш выбор: ");

            string input = (Console.ReadLine() ?? "").Trim();

            return input switch
            {
                "" => RoundMenuChoice.NextRound,
                "1" => RoundMenuChoice.ShowArmyState,
                "2" => RoundMenuChoice.SaveAndExit,
                "3" => RoundMenuChoice.AutoMode,
                "4" => RoundMenuChoice.ExitWithoutSave,
                "5" => RoundMenuChoice.ChangeFormation,
                "6" => RoundMenuChoice.Undo,
                "7" => RoundMenuChoice.Redo,
                "8" => RoundMenuChoice.Reset,
                "9" => RoundMenuChoice.ShowHistory,
                _ => RoundMenuChoice.Unknown
            };
        }

        public void PrintArmyState(IArmy army1, IArmy army2)
        {
            Console.WriteLine();
            Console.WriteLine($"Состав армии {army1.Name}:");
            Thread.Sleep(30);
            foreach (var unit in army1.Units)
            {
                Console.WriteLine($"  {unit.Name} (HP:{unit.Health}/{unit.MaxHealth}, ATK:{unit.Attack}, DEF:{unit.Defence})");
                Thread.Sleep(30);
            }
            Console.WriteLine();
            Console.WriteLine($"Состав армии {army2.Name}:");
            Thread.Sleep(30);
            foreach (var unit in army2.Units)
            {
                Console.WriteLine($"  {unit.Name} (HP:{unit.Health}/{unit.MaxHealth}, ATK:{unit.Attack}, DEF:{unit.Defence})");
                Thread.Sleep(30);
            }
            Console.WriteLine();
        }

        public void PrintHistory(IEnumerable<string> entries)
        {
            Console.WriteLine();
            Console.WriteLine("История действий:");
            var list = entries.ToList();
            if (list.Count == 0) { Console.WriteLine("  История пуста."); return; }
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine($"  {i + 1}. {list[i]}");
        }

        public void PrintSaved(string fileName) =>
            Console.WriteLine($"Игра сохранена: {fileName}");

        public void PrintSaveFailed() =>
            Console.WriteLine("Сохранение недоступно: логгер не поддерживает запись.");

        public void PrintMessage(string message) =>
            Console.WriteLine(message);

        public string ReadSaveName()
        {
            Console.Write("Введите название сохранения: ");
            return (Console.ReadLine() ?? "").Trim();
        }

        public IBattleFormation? ReadFormationChoice()
        {
            Console.WriteLine("Выберите построение:");
            Console.WriteLine("1. Бой на мосту");
            Console.WriteLine("2. Бой на широком мосту");
            Console.WriteLine("3. Стенка на стенку");
            Console.Write("Ваш выбор: ");
            return Console.ReadLine()?.Trim() switch
            {
                "1" => new BridgeFormation(),
                "2" => new WideBridgeFormation(),
                "3" => new WallFormation(),
                _ => null
            };
        }
    }
}