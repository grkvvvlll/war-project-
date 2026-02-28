using System;
using System.Collections.Generic;
using System.Linq;
using gaaameee.Core.Interfaces;
using gaaameee.Core.Entities;
using gaaameee.Core.Factories;
using Services.Battle;
using Services.Logging;
using Services.Random;
using Services.Storage;

namespace Presentation
{
    public class ConsoleMenu
    {
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleField _battleField;

        public ConsoleMenu(
            IRandomService random,
            IBattleLogger logger,
            IDamageCalculator damageCalculator,
            IBattleField battleField)
        {
            _random = random;
            _logger = logger;
            _damageCalculator = damageCalculator;
            _battleField = battleField;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Army Game ===");
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Помощь");
                Console.WriteLine("3. Загрузить игру");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        StartNewGame();
                        break;

                    case "2":
                        ShowHelp();
                        break;

                    case "3":
                        LoadGame();
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

        private void StartNewGame()
        {
            Console.Clear();
            if (_logger is RecordingBattleLogger rec)
                rec.Clear();

            Console.Write("Введите количество юнитов в армии 1: ");
            int count1 = ReadInt();

            Console.Write("Введите количество юнитов в армии 2: ");
            int count2 = ReadInt();

            var army1 = CreateRandomArmy("Армия 1", count1);
            var army2 = CreateRandomArmy("Армия 2", count2);

            Console.Clear();
            Console.WriteLine("Армии готовы:\n");

            BattleVisualizer.PrintArmyLine(army1, army2);

            Console.WriteLine("\nНажмите Enter для начала боя...");
            Console.ReadLine();

            var result = _battleField.StartBattle(army1, army2);

            Console.WriteLine($"\nПобедитель: {result.Winner}");
            Console.WriteLine($"Ходов: {result.Turns}");

            AskToSaveBattle(result);

            Console.ReadLine();
        }

        private IArmy CreateRandomArmy(string name, int count)
        {
            var units = new List<IUnit>();

            for (int i = 0; i < count; i++)
            {
                int type = _random.Next(0, 3);

                switch (type)
                {
                    case 0:
                        units.Add(UnitFactory.CreateHeavy($"Heavy {i + 1}"));
                        break;

                    case 1:
                        units.Add(UnitFactory.CreateLight($"Light {i + 1}"));
                        break;

                    case 2:
                        units.Add(UnitFactory.CreateArcher($"Archer {i + 1}"));
                        break;
                }
            }

            return new Army(name, units);
        }

        private void ShowHelp()
        {
            Console.Clear();

            var heavy = UnitFactory.CreateHeavy("Heavy");
            var light = UnitFactory.CreateLight("Light");
            var archer = UnitFactory.CreateArcher("Archer");

            PrintUnitInfo("🛡️ HeavyUnit - сильный солдат:", heavy);
            PrintUnitInfo("⚔️ LightUnit - обычный солдат:", light);
            PrintUnitInfo("🏹 Archer - лучник:", archer);

            Console.WriteLine("Алгоритм игры:");
            Console.WriteLine("Случайным образом выбирается армия, атакующая первой.");
            Console.WriteLine("Ближайшие друг к другу солдаты вражеских армий наносят по одному удару.");
            Console.WriteLine("Лучники обеих армий, начиная со второго, стреляют по противнику, если могут.");
            Console.WriteLine("Убитые солдаты исчезают.");

            Console.WriteLine("\nНажмите Enter для возврата в меню");
            Console.ReadLine();
        }

        private void PrintUnitInfo(string title, IUnit unit)
        {
            Console.WriteLine(title);
            Console.WriteLine($"   HP: {unit.Health}");
            Console.WriteLine($"   ATK: {unit.Attack}");
            Console.WriteLine($"   DEF: {unit.Defence}");
            Console.WriteLine($"   COST: {unit.Cost}");

            if (unit is Archer archer)
                Console.WriteLine($"   RANGE: {archer.Range}");

            Console.WriteLine();
        }

        private int ReadInt()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                    return value;

                Console.Write("Введите корректное число: ");
            }
        }

        // =========================
        // SAVE / LOAD
        // =========================

        private void AskToSaveBattle(BattleResult result)
        {
            if (_logger is not RecordingBattleLogger rec)
            {
                Console.WriteLine("\n(Сохранение недоступно: логгер не RecordingBattleLogger)");
                return;
            }

            Console.Write("\nСохранить бой в файл? (y/n): ");
            var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            if (ans != "y" && ans != "yes" && ans != "д" && ans != "да")
                return;

            var saveService = new BattleSaveService();
            var save = new BattleSave
            {
                Winner = result.Winner,
                Turns = result.Turns,
                LogLines = rec.Lines.ToList()
            };

            var fileName = saveService.Save(save);
            Console.WriteLine($"Сохранено: saves/{fileName}");
        }

        private void LoadGame()
        {
            Console.Clear();

            var saveService = new BattleSaveService();
            var saves = saveService.ListSaves();

            if (saves.Count == 0)
            {
                Console.WriteLine("Сохранений нет. Нажмите Enter.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("=== Сохранённые бои ===");
            for (int i = 0; i < saves.Count; i++)
            {
                var s = saves[i];
                Console.WriteLine($"{i + 1}. {s.FileName} | {s.SavedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Winner: {s.Winner} | Turns: {s.Turns}");
            }

            Console.Write("\nВведите номер сохранения (0 - назад): ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n < 0 || n > saves.Count)
            {
                Console.WriteLine("Неверный ввод. Enter...");
                Console.ReadLine();
                return;
            }

            if (n == 0) return;

            var chosen = saves[n - 1];
            var save = saveService.Load(chosen.FileName);

            Console.Clear();
            Console.WriteLine($"=== Бой из файла: {chosen.FileName} ===");
            Console.WriteLine($"SavedAt (UTC): {save.SavedAtUtc:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Winner: {save.Winner}");
            Console.WriteLine($"Turns: {save.Turns}");
            Console.WriteLine("\n--- LOG ---\n");

            foreach (var line in save.LogLines)
                Console.WriteLine(line);

            Console.WriteLine("\n--- END ---");
            Console.WriteLine("Нажмите Enter для возврата в меню...");
            Console.ReadLine();
        }
    }
}