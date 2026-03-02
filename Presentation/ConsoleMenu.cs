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

        // распределение бюджета на юнитов
        private const double HeavyWeight = 0.3;
        private const double LightWeight = 0.4;
        private const double ArcherWeight = 0.3;

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

            Console.Write("Введите стоимость для армии 1: ");
            int budget1 = ReadInt();
            Console.Write("Введите стоимость для армии 2: ");
            int budget2 = ReadInt();

            // генерация армий по бюджету
            var army1 = CreateArmyByBudget("Армия 1", budget1);
            var army2 = CreateArmyByBudget("Армия 2", budget2);

            Console.Clear();
            Console.WriteLine("Армии сформированы:");
            Console.WriteLine();

            PrintArmyComposition(army1);
            Console.WriteLine();
            PrintArmyComposition(army2);
            Console.WriteLine();

            Console.WriteLine("Нажмите Enter для начала боя...");
            Console.ReadLine();

            var result = _battleField.StartBattle(army1, army2);

            Console.WriteLine($"\nПобедитель: {result.Winner}");
            Console.WriteLine($"Ходов: {result.Turns}");
            AskToSaveBattle(result);
            Console.ReadLine();
        }

        // метод для создания армий по бюджету
        private IArmy CreateArmyByBudget(string name, int budget)
        {
            var units = new List<IUnit>();
            var random = new System.Random();

            // типы юнитов с их весами и стоимостью
            var unitTypes = new List<(string type, int cost, double weight)>
            {
                ("Heavy", UnitFactory.HeavyCost, HeavyWeight),
                ("Light", UnitFactory.LightCost, LightWeight),
                ("Archer", UnitFactory.ArcherCost, ArcherWeight)
            };

            var budgets = new int[unitTypes.Count];
            for (int i = 0; i < unitTypes.Count; i++)
            {
                int baseBudget = (int)(budget * (unitTypes[i].weight / (HeavyWeight + LightWeight + ArcherWeight)));
                int variance = (int)(baseBudget * 0.15);
                int variation = random.Next(-variance, variance + 1);
                budgets[i] = Math.Max(0, baseBudget + variation);
            }

            // нумерация внутри армий
            var counters = new Dictionary<string, int>
            {
                { "Heavy", 0 },
                { "Light", 0 },
                { "Archer", 0 }
            };

            // создаем юнитов согласно бюджету
            for (int i = 0; i < unitTypes.Count; i++)
            {
                int count = budgets[i] / unitTypes[i].cost;
                for (int j = 0; j < count; j++)
                {
                    counters[unitTypes[i].type]++;
                    units.Add(CreateUnitByType(unitTypes[i].type, counters[unitTypes[i].type]));
                }
            }

            // остаток бюджета случайные доступные юниты 
            int remaining = budget - units.Sum(u => u.Cost);
            while (remaining >= unitTypes.Min(t => t.cost))
            {
                var affordable = unitTypes.Where(t => t.cost <= remaining).ToList();
                var chosen = affordable[random.Next(affordable.Count)];
                counters[chosen.type]++;
                units.Add(CreateUnitByType(chosen.type, counters[chosen.type]));
                remaining -= chosen.cost;
            }

            // если армия пустая — добавим хотя бы одного дешёвого юнита 
            if (units.Count == 0 && budget > 0)
            {
                var cheapest = unitTypes.OrderBy(t => t.cost).First();
                counters[cheapest.type]++;
                units.Add(CreateUnitByType(cheapest.type, counters[cheapest.type]));
            }

            return new Army(name, units);
        }

        private IUnit CreateUnitByType(string type, int number)
        {
            return type switch
            {
                "Heavy" => UnitFactory.CreateHeavy($"Heavy {number}"),
                "Light" => UnitFactory.CreateLight($"Light {number}"),
                "Archer" => UnitFactory.CreateArcher($"Archer {number}"),
                _ => UnitFactory.CreateLight($"Light {number}")
            };
        }

        private void PrintArmyComposition(IArmy army)
        {
            Console.WriteLine($"=== {army.Name} (Бюджет: {army.TotalCost} монет) ===");

            var heavyCount = army.Units.Count(u => u is HeavyUnit);
            var lightCount = army.Units.Count(u => u is LightUnit);
            var archerCount = army.Units.Count(u => u is Archer);

            Console.WriteLine($"🛡️ Тяжёлых: {heavyCount} × {UnitFactory.HeavyCost} = {heavyCount * UnitFactory.HeavyCost} монет");
            Console.WriteLine($"⚔️ Лёгких: {lightCount} × {UnitFactory.LightCost} = {lightCount * UnitFactory.LightCost} монет");
            Console.WriteLine($"🏹 Лучников: {archerCount} × {UnitFactory.ArcherCost} = {archerCount * UnitFactory.ArcherCost} монет");
            Console.WriteLine($"─────────────────────────────────────────");
            Console.WriteLine($"Всего юнитов: {army.Units.Count}");
            Console.WriteLine($"Итого потрачено: {army.TotalCost} монет");

            // Показываем всех юнитов
            Console.WriteLine("\nСостав армии:");
            foreach (var unit in army.Units)
            {
                string icon = unit switch
                {
                    HeavyUnit _ => "🛡️",
                    LightUnit _ => "⚔️",
                    Archer _ => "🏹",
                    _ => "❓"
                };
                Console.WriteLine($"  {icon} {unit.Name} (HP:{unit.Health} ATK:{unit.Attack} DEF:{unit.Defence})");
            }
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
                Console.WriteLine($"{i + 1}. {s.FileName} | {s.SavedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Победитель: {s.Winner} | Ходов: {s.Turns}");
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
            Console.WriteLine($"Сохранено (UTC): {save.SavedAtUtc:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Победитель: {save.Winner}");
            Console.WriteLine($"Ходов: {save.Turns}");
            foreach (var line in save.LogLines)
                Console.WriteLine(line);
            Console.WriteLine("Нажмите Enter для возврата в меню...");
            Console.ReadLine();
        }
    }
}