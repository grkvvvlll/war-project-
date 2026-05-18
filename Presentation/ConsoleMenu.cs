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
        private readonly UnitRenumberer _unitRenumberer;
        private readonly BudgetReader _budgetReader;
        private readonly HelpPrinter _helpPrinter;
        private readonly CreationTypeSelector _creationTypeSelector;
        private readonly ObserverSettingsMenu _observerSettingsMenu;
        private readonly ObserverAttacher _observerAttacher;



        public ConsoleMenu(
            IRandomService random,
            IBattleLogger logger,
            IDamageCalculator damageCalculator,
            IBattleField battleField,
            ArmyBuilder armyBuilder,
            FormationSelector formationSelector,
            LogCleaner logCleaner,
            ArmyPrinter armyPrinter,
            UnitRenumberer unitRenumberer,
            CreationTypeSelector creationTypeSelector,
            ObserverAttacher observerAttacher,
            BudgetReader budgetReader)
            
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
            _unitRenumberer = new UnitRenumberer();
            _budgetReader = new BudgetReader();
            _helpPrinter = new HelpPrinter(random);
            _creationTypeSelector = new CreationTypeSelector();
            _observerSettingsMenu = new ObserverSettingsMenu();
            _observerAttacher = new ObserverAttacher();
            _budgetReader = budgetReader;
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
                        _observerSettingsMenu.Show();
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

            int budget = _budgetReader.Read();
            Console.WriteLine();

            var formation = _formationSelector.Select();
            if (_battleField is BattleField bf) bf.SetFormation(formation);

            var army1 = _armyBuilder.Build("Армия 1", budget, _creationTypeSelector.Select("Армия 1"));
            _unitRenumberer.Renumber(army1, true, formation);

            var army2 = _armyBuilder.Build("Армия 2", budget, _creationTypeSelector.Select("Армия 2"));
            _unitRenumberer.Renumber(army2, false, formation);

            _observerAttacher.AttachArmy(army1);
            _observerAttacher.AttachArmy(army2);

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
                _observerAttacher.DetachArmy(army1);
                _observerAttacher.DetachArmy(army2);
            }
        }


        private void ShowHelp()
        {
            _logCleaner.Clear();
            Console.Clear();
            _helpPrinter.Print();
            Console.WriteLine("\nНажмите Enter для возврата в меню");
            Console.ReadLine();
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

        
    }
}