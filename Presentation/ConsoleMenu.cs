using Core.Entities;
using Core.Entities.Units;
using Core.Factories;
using Core.Factories.Armies;
using Core.Factories.Units;
using Core.Formations;
using Core.Interfaces;
using Services.Battle;
using Services.Logging;
using Services.Random;
using Services.Storage;
using Services.Observers;
using Services.ArmyBuilding;
using Services.Formation;
using Services.UI;

namespace Presentation
{
    public class ConsoleMenu
    {
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleField _battleField;
        private readonly UnitCreatorFactory _unitCreatorFactory;
        private readonly ArmyBuilder _armyBuilder;
        private readonly FormationSelector _formationSelector;
        private readonly LogCleaner _logCleaner;
        private readonly ArmyPrinter _armyPrinter;

        public ConsoleMenu(
            IRandomService random,
            IBattleLogger logger,
            IDamageCalculator damageCalculator,
            IBattleField battleField,
            ArmyBuilder armyBuilder,
            FormationSelector formationSelector,
            LogCleaner logCleaner,
            ArmyPrinter armyPrinter)
        {
            _random = random;
            _logger = logger;
            _damageCalculator = damageCalculator;
            _battleField = battleField;
            _armyBuilder = armyBuilder;
            _formationSelector = formationSelector;
            _logCleaner = logCleaner;
            _armyPrinter = armyPrinter;
            _unitCreatorFactory = new UnitCreatorFactory(random);
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Помощь");
                Console.WriteLine("3. Загрузить игру");
                Console.WriteLine("4. Настройки наблюдателей");
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
                    case "4":
                        ShowObserverSettings();
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

        private void ShowObserverSettings()
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

        private void StartNewGame()
        {
            _logCleaner.Clear();
            Console.Clear();

            Console.Write("Введите бюджет для армий: ");
            int budget = ReadInt();
            Console.WriteLine();

            var formation = _formationSelector.Select();
            if (_battleField is BattleField bf) bf.SetFormation(formation);

            var army1 = _armyBuilder.Build("Армия 1", budget, ChooseCreationType("Армия 1"));
            RenumberUnitsFromFront(army1, isArmy1: true);

            var army2 = _armyBuilder.Build("Армия 2", budget, ChooseCreationType("Армия 2"));
            RenumberUnitsFromFront(army2, isArmy1: false);
            ObserverRegistry.Attach(army1);
            ObserverRegistry.Attach(army2);

            try
            {
                Console.Clear();
                Console.WriteLine("Армии сформированы:");
                Console.WriteLine();
                _armyPrinter.Print(army1);
                Console.WriteLine();
                _armyPrinter.Print(army2);
                Console.WriteLine();
                Console.WriteLine("Нажмите Enter для начала боя...");
                Console.ReadLine();

                var result = _battleField.StartBattle(army1, army2, autoMode: false);

                if (result.Winner == BattleField.DrawResult)
                {
                    Console.WriteLine($"\n Ничья после {result.Turns} ходов!");
                    AskToSaveBattle(result);
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
            }
            finally
            {
                ObserverRegistry.Detach(army1);
                ObserverRegistry.Detach(army2);
            }
        }

        private bool ChooseCreationType(string armyName)
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

        private void ShowHelp()
        {
            _logCleaner.Clear();
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
                Console.WriteLine("Нажмите Enter для возврата в меню...");
                Console.ReadLine();
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
            Console.WriteLine("Нажмите Enter для возврата в меню...");
            Console.ReadLine();
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

            if (_battleField is BattleField bf)
                bf.SetFormation(restored.Formation);

            ObserverRegistry.Attach(restored.Army1);
            ObserverRegistry.Attach(restored.Army2);

            try
            {
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
            }
            finally
            {
                ObserverRegistry.Detach(restored.Army1);
                ObserverRegistry.Detach(restored.Army2);
            }
        }

        private void RenumberUnitsFromFront(IArmy army, bool isArmy1)
        {
            var aliveUnits = army.Units.Where(u => u.IsAlive).ToList();

            bool wallFormation = _battleField is BattleField bf && bf.GetFormation() is WallFormation;

            if (isArmy1 && !wallFormation)
            {
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[aliveUnits.Count - 1 - i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
            else
            {
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
        }
    }
}