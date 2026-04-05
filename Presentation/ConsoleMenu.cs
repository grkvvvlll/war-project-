using System;
using Core.Entities;
using Core.Entities.Units;
using Core.Factories;
using Core.Factories.Armies;
using Core.Factories.Units;
using Core.Interfaces;
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

        // factory метод для создания юнитов
        private readonly Dictionary<string, UnitCreator> _unitCreators;
        private readonly AutoArmyFactory _autoFactory;
        private readonly ManualArmyFactory _manualFactory;

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

            // ИНИЦИАЛИЗАЦИЯ FACTORY METHOD 
            _unitCreators = new Dictionary<string, UnitCreator>
            {
                { "Heavy", new HeavyUnitCreator() },
                { "Light", new LightUnitCreator(random) },
                { "Archer", new ArcherUnitCreator() },
                { "Healer", new HealerUnitCreator() },
                { "Wizard", new WizardUnitCreator(random) },
                { "GulyayGorod", new GulyayGorodCreator() }
            };

            _autoFactory = new AutoArmyFactory(_unitCreators);
            _manualFactory = new ManualArmyFactory(_unitCreators);
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

            Console.Write("Введите бюджет для армий: ");
            int budget = ReadInt();

            // === 2. АРМИЯ 1 ===
            Console.WriteLine("\n=== АРМИЯ 1 ===");
            var army1 = CreateArmyWithChoice("Армия 1", budget);
            RenumberUnitsFromFront(army1, isArmy1: true);

            // === 3. АРМИЯ 2 ===
            Console.WriteLine("\n=== АРМИЯ 2 ===");
            var army2 = CreateArmyWithChoice("Армия 2", budget);
            RenumberUnitsFromFront(army2, isArmy1: false);

            Console.Clear();
            Console.WriteLine("Армии сформированы:");
            Console.WriteLine();
            PrintArmyComposition(army1);
            Console.WriteLine();
            PrintArmyComposition(army2);
            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для начала боя...");
            Console.ReadLine();

            var result = _battleField.StartBattle(army1, army2, autoMode: false);

            if (result.Winner == BattleField.DrawResult)  // ← НОВОЕ
            {
                Console.WriteLine($"\n Ничья после {result.Turns} ходов!");
                AskToSaveBattle(result);
                Console.ReadLine();
                return;
            }

            if (result.Winner == BattleField.SavedAndStoppedResult)
            {
                Console.WriteLine($"\nИгра сохранена. Бой остановлен на {result.Turns}-м раунде.");
                Console.WriteLine("Нажмите Enter для возврата в меню...");
                Console.ReadLine();
                return;
            }
            if (result.Winner == BattleField.StoppedWithoutSaveResult)
            {
                Console.WriteLine($"\nБой остановлен на {result.Turns}-м раунде без сохранения.");
                Console.WriteLine("Нажмите Enter для возврата в меню...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\nПобедитель: {result.Winner}");
            Console.WriteLine($"Ходов: {result.Turns}");
            AskToSaveBattle(result);
            Console.ReadLine();
        }

        private IArmy CreateArmyWithChoice(string armyName, int budget)
        {
            while (true)
            {
                Console.WriteLine($"\n{armyName}:");
                Console.WriteLine("1. Автоматическое создание");
                Console.WriteLine("2. Ручное создание");
                Console.Write("Выберите способ (1-2): ");

                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    return _autoFactory.CreateArmy(armyName, budget);
                }
                else if (choice == "2")
                {
                    var unitChoices = GetManualUnitChoices(armyName, budget);
                    return _manualFactory.CreateArmy(armyName, budget, unitChoices);
                }
                else
                {
                    // Некорректный ввод
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Неверный ввод. Пожалуйста, введите 1 или 2.");
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
        }

        private List<string> GetManualUnitChoices(string armyName, int budget)
        {
            var choices = new List<string>();
            int spentBudget = 0;
            int minCost = _manualFactory.GetMinUnitCost();

            Console.WriteLine($"\n=== {armyName} (Бюджет: {budget} монет) ===");

            while (spentBudget + minCost <= budget)
            {
                int remaining = budget - spentBudget;
                Console.WriteLine($"\nОсталось: {remaining} монет");
                Console.WriteLine("Выберите юнита:");
                Console.WriteLine($"1. Heavy ({_manualFactory.GetUnitCost("Heavy")} монет)");
                Console.WriteLine($"2. Light ({_manualFactory.GetUnitCost("Light")} монет)");
                Console.WriteLine($"3. Archer ({_manualFactory.GetUnitCost("Archer")} монет)");
                Console.WriteLine($"4. Healer ({_manualFactory.GetUnitCost("Healer")} монет)");
                Console.WriteLine($"5. Wizard ({_manualFactory.GetUnitCost("Wizard")} монет)");
                Console.WriteLine($"6. GulyayGorod ({_manualFactory.GetUnitCost("GulyayGorod")} монет)");
                Console.WriteLine("0. Закончить формирование");
                Console.Write("Ваш выбор: ");
                var input = Console.ReadLine();

                if (input == "0")
                    break;

                var unitType = input switch
                {
                    "1" => "Heavy",
                    "2" => "Light",
                    "3" => "Archer",
                    "4" => "Healer",
                    "5" => "Wizard",
                    "6" => "GulyayGorod",
                    _ => null
                };

                if (unitType == null)
                {
                    Console.WriteLine("  Неверный выбор");
                    continue;
                }

                int cost = _manualFactory.GetUnitCost(unitType);
                if (cost > remaining)
                {
                    Console.WriteLine("  Недостаточно средств");
                    continue;
                }

                choices.Add(unitType);
                spentBudget += cost;
                Console.WriteLine($"  Добавлен {unitType} (-{cost} монет)");
            }

            if (choices.Count == 0)
            {
                Console.WriteLine("  Армия пуста, добавлен юнит по умолчанию");
                choices.Add("Light");
            }

            return choices;
        }

        private void PrintArmyComposition(IArmy army)
        {
            Console.WriteLine($"=== {army.Name} (Бюджет: {army.TotalCost} монет) ===");
            var heavyCount = army.Units.Count(u => u is HeavyUnit);
            var lightCount = army.Units.Count(u => u is LightUnit);
            var archerCount = army.Units.Count(u => u is Archer);
            var healerCount = army.Units.Count(u => u is Healer);
            var wizardCount = army.Units.Count(u => u is Wizard);
            var gulyayCount = army.Units.Count(u => u is GulyayGorodAdapter);

            Console.WriteLine($"🛡️ Тяжёлых: {heavyCount} × {UnitFactory.HeavyCost} = {heavyCount * UnitFactory.HeavyCost} монет");
            Console.WriteLine($"⚔️ Лёгких: {lightCount} × {UnitFactory.LightCost} = {lightCount * UnitFactory.LightCost} монет");
            Console.WriteLine($"🏹 Лучников: {archerCount} × {UnitFactory.ArcherCost} = {archerCount * UnitFactory.ArcherCost} монет");
            Console.WriteLine($"💚 Целителей: {healerCount} × {UnitFactory.HealerCost} = {healerCount * UnitFactory.HealerCost} монет");
            Console.WriteLine($"🔮 Магов: {wizardCount} × {UnitFactory.WizardCost} = {wizardCount * UnitFactory.WizardCost} монет");
            Console.WriteLine($"🏰 Гуляй-город: {gulyayCount} × {UnitFactory.GulyayGorodCost} = {gulyayCount * UnitFactory.GulyayGorodCost} монет");
            Console.WriteLine($"─────────────────────────────────────────");
            Console.WriteLine($"Всего юнитов: {army.Units.Count}");
            Console.WriteLine($"Итого потрачено: {army.TotalCost} монет");

            Console.WriteLine("\nСостав армии:");
            foreach (var unit in army.Units)
            {
                string icon = unit switch
                {
                    HeavyUnit _ => "🛡️",
                    LightUnit _ => "⚔️",
                    Archer _ => "🏹",
                    Healer _ => "💚",
                    Wizard _ => "🔮",
                    GulyayGorodAdapter _ => "🏰",
                    _ => "❓"
                };
                Console.WriteLine($"  {icon} {unit.Name} (HP:{unit.Health} ATK:{unit.Attack} DEF:{unit.Defence})");
            }
        }

        private void ShowHelp()
        {
            Console.Clear();

            var heavy = new HeavyUnitCreator().CreateUnit("Heavy");
            var light = new LightUnitCreator(_random).CreateUnit("Light");
            var archer = new ArcherUnitCreator().CreateUnit("Archer");
            var healer = new HealerUnitCreator().CreateUnit("Healer");
            var wizard = new WizardUnitCreator(new RandomService()).CreateUnit("Wizard");

            PrintUnitInfo("🛡️ HeavyUnit - сильный солдат:", heavy);
            PrintUnitInfo("⚔️ LightUnit - обычный солдат:", light);
            PrintUnitInfo("🏹 Archer - лучник:", archer);
            PrintUnitInfo("💚 Healer - целитель:", healer);
            PrintUnitInfo("🔮 Wizard - маг:", wizard);
            Console.WriteLine("🏰 Гуляй-город: огромная защита, не атакует, не лечится, не клонируется.\n");

            Console.WriteLine("Алгоритм игры:");
            Console.WriteLine("1. Случайным образом выбирается армия, атакующая первой.");
            Console.WriteLine("2. Ближайшие друг к другу солдаты вражеских армий наносят по одному удару.");
            Console.WriteLine("3. Юниты со SpecialAbility используют свои способности:");
            Console.WriteLine("   - 🏹 Лучники стреляют во врагов (если не на передней линии).");
            Console.WriteLine("   - 💚 Целители лечат союзников (кроме Heavy и себя).");
            Console.WriteLine("   - 🔮 Маги клонируют союзников (Light или Archer) с накоплением вероятности.");
            Console.WriteLine("4. Убитые солдаты исчезают.");
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
            if (unit is Healer healer)
            {
                Console.WriteLine($"   HEAL_RANGE: {healer.HealRange}");
                Console.WriteLine($"   HEAL_POWER: {healer.HealPower}");
            }
            if (unit is Wizard wizard)
            {
                Console.WriteLine($"   SPELL_RANGE: {wizard.SpellRange}");
                Console.WriteLine($"   CLONE_CHANCE: {wizard.ClonePower}%");
            }
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
            if (_logger is not IRecordingBattleLogger rec)
            {
                Console.WriteLine("\n(Сохранение недоступно: логгер не RecordingBattleLogger)");
                return;
            }

            Console.Write("\nСохранить бой в файл? (y/n): ");
            var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (ans != "y" && ans != "yes" && ans != "д" && ans != "да")
                return;

            Console.Write("Введите название сохранения: ");
            string saveName = (Console.ReadLine() ?? "").Trim();

            var saveService = BattleSaveService.Instance;
            var save = saveService.CreateFinishedSave(result, rec.Lines, saveName);
            var fileName = saveService.Save(save, saveName);

            Console.WriteLine($"Сохранено: saves/{fileName}");
        }

        private void LoadGame()
        {
            Console.Clear();

            var saveService = BattleSaveService.Instance;
            var saves = saveService.ListSaves();

            if (saves.Count == 0)
            {
                Console.WriteLine("Сохранений нет. Нажмите Enter.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("=== Сохранения ===");
            for (int i = 0; i < saves.Count; i++)
            {
                var s = saves[i];
                var winnerText = string.IsNullOrWhiteSpace(s.Winner) ? "бой не завершён" : s.Winner;
                var displayName = string.IsNullOrWhiteSpace(s.DisplayName) ? s.FileName : s.DisplayName;

                Console.WriteLine(
                    $"{i + 1}. {displayName} | {s.SavedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Победитель: {winnerText} | Ходов: {s.Turns}");
            }

            Console.Write("\nВведите номер сохранения (0 - назад): ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n < 0 || n > saves.Count)
            {
                Console.WriteLine("Неверный ввод. Нажмите Enter.");
                Console.ReadLine();
                return;
            }

            if (n == 0)
                return;

            var chosen = saves[n - 1];
            var save = saveService.Load(chosen.FileName);

            Console.Clear();
            Console.WriteLine($"=== Загрузка: {chosen.FileName} ===");
            Console.WriteLine($"Сохранено (UTC): {save.SavedAtUtc:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Ходов уже сыграно: {save.Turns}");

            if (_logger is RecordingBattleLogger rec)
            {
                rec.Clear();
                foreach (var line in save.LogLines)
                    rec.Lines.Add(line);
            }

            if (save.IsFinished)
            {
                Console.WriteLine($"Победитель: {save.Winner}");
                Console.WriteLine();

                foreach (var line in save.LogLines)
                    Console.WriteLine(line);

                Console.WriteLine("\nЭто завершённый бой. Нажмите Enter для возврата в меню...");
                Console.ReadLine();
                return;
            }

            var restored = saveService.RestoreBattle(save, _random);

            Console.WriteLine("Бой восстановлен.");
            Console.WriteLine($"Следующий раунд: {restored.Turns + 1}");
            Console.WriteLine($"Следующей атакует: {(restored.Army1Turn ? restored.Army1.Name : restored.Army2.Name)}");
            Console.WriteLine($"Счёт: {restored.ScoreArmy1} : {restored.ScoreArmy2}");
            Console.WriteLine();
            Console.WriteLine("Нажмите Enter, чтобы продолжить бой...");
            Console.ReadLine();

            var result = _battleField.StartBattle(
                restored.Army1,
                restored.Army2,
                restored.Turns,
                restored.Army1Turn,
                restored.ScoreArmy1,
                restored.ScoreArmy2,
                autoMode: false,
                showRoundMenuBeforeFirstRound: false);

            if (result.Winner == BattleField.SavedAndStoppedResult)
            {
                Console.WriteLine($"\nИгра сохранена. Бой остановлен на {result.Turns}-м раунде.");
                Console.WriteLine("Нажмите Enter для возврата в меню...");
                Console.ReadLine();
                return;
            }

            if (result.Winner == BattleField.StoppedWithoutSaveResult)
            {
                Console.WriteLine($"\nБой остановлен на {result.Turns}-м раунде без сохранения.");
                Console.WriteLine("Нажмите Enter для возврата в меню...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\nПобедитель: {result.Winner}");
            Console.WriteLine($"Ходов: {result.Turns}");
            AskToSaveBattle(result);
            Console.ReadLine();
        }

        // Перенумерация юнитов от фронта ===
        private void RenumberUnitsFromFront(IArmy army, bool isArmy1)
        {
            var aliveUnits = army.Units.Where(u => u.IsAlive).ToList();

            if (isArmy1)
            {
                // Армия 1 (левая): фронт СПРАВА, нумерация СПРАВА НАЛЕВО
                // Юнит справа (индекс Count-1) получает номер 1
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[aliveUnits.Count - 1 - i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}"; // Прямое присваивание
                }
            }
            else
            {
                // Армия 2 (правая): фронт СЛЕВА, нумерация СЛЕВА НАПРАВО
                // Юнит слева (индекс 0) получает номер 1
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}"; // Прямое присваивание
                }
            }
        }
    }
}