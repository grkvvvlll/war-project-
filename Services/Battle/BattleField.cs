using Core.Entities;
using Core.Interfaces;
using Services.Storage;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        public const string SavedAndStoppedResult = "Бой сохранён и остановлен";

        private readonly IMeleeService _meleeService;
        private readonly SpecialAbilityService _specialAbilityService;
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;

        private int _scoreArmy1 = 0;
        private int _scoreArmy2 = 0;

        public BattleField(
            IMeleeService meleeService,
            SpecialAbilityService specialAbilityService,
            IRandomService random,
            IBattleLogger logger)
        {
            _meleeService = meleeService;
            _specialAbilityService = specialAbilityService;
            _random = random;
            _logger = logger;
        }

        public BattleResult StartBattle(
            IArmy army1,
            IArmy army2,
            int turns = 0,
            bool? army1Turn = null,
            int scoreArmy1 = 0,
            int scoreArmy2 = 0)
        {
            _scoreArmy1 = scoreArmy1;
            _scoreArmy2 = scoreArmy2;

            bool currentArmy1Turn = army1Turn ?? (_random.Next(0, 2) == 0);

            if (turns == 0)
            {
                _logger.LogInfo($"Первой атакует: {(currentArmy1Turn ? army1.Name : army2.Name)}");
            }
            else
            {
                _logger.LogInfo($"Бой продолжен с {turns + 1}-го раунда.");
                _logger.LogInfo($"Следующей атакует: {(currentArmy1Turn ? army1.Name : army2.Name)}");
                _logger.LogInfo($"Текущий счёт: {_scoreArmy1} : {_scoreArmy2}");
            }

            if (!WaitForRoundAction(army1, army2, turns, currentArmy1Turn))
                return new BattleResult(SavedAndStoppedResult, turns);

            while (HasAlive(army1) && HasAlive(army2))
            {
                BattleVisualizer.PrintArmyLine(army1, army2);
                Console.WriteLine();

                if (currentArmy1Turn)
                {
                    _scoreArmy1 += _meleeService.Execute(army1, army2, true);

                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _meleeService.Execute(army2, army1, false);

                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _specialAbilityService.Execute(army1, army2, true);

                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _specialAbilityService.Execute(army2, army1, false);
                }
                else
                {
                    _scoreArmy2 += _meleeService.Execute(army2, army1, false);

                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _meleeService.Execute(army1, army2, true);

                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _specialAbilityService.Execute(army2, army1, false);

                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _specialAbilityService.Execute(army1, army2, true);
                }

                _logger.LogInfo($"СЧЁТ: {_scoreArmy1} : {_scoreArmy2}");

                army1.RemoveDeadUnits();
                army2.RemoveDeadUnits();

                turns++;
                currentArmy1Turn = !currentArmy1Turn;

                if (HasAlive(army1) && HasAlive(army2))
                {
                    if (!WaitForRoundAction(army1, army2, turns, currentArmy1Turn))
                        return new BattleResult(SavedAndStoppedResult, turns);
                }
            }

            string winner = HasAlive(army1) ? army1.Name : army2.Name;
            return new BattleResult(winner, turns);
        }

        private bool HasAlive(IArmy army)
        {
            return army.Units.Any(u => u.IsAlive);
        }

        private bool WaitForRoundAction(IArmy army1, IArmy army2, int turns, bool army1Turn)
        {
            while (true)
            {
                int shownRound = turns + 1;

                Console.WriteLine();
                Console.WriteLine($"МЕНЮ (перед {shownRound}-м раундом)");
                Console.WriteLine("Enter - следующий раунд");
                Console.WriteLine("1 - показать состав армий");
                Console.WriteLine("2 - сохранить и выйти в меню");
                Console.Write("Ваш выбор: ");

                string input = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrEmpty(input))
                    return true;

                if (input == "1")
                {
                    PrintArmyState(army1, army2);
                    continue;
                }

                if (input == "2")
                {
                    SaveBattle(army1, army2, turns, army1Turn);
                    return false;
                }

                Console.WriteLine("Неизвестная команда.");
            }
        }

        private void SaveBattle(IArmy army1, IArmy army2, int turns, bool army1Turn)
        {
            if (_logger is not IRecordingBattleLogger rec)
            {
                Console.WriteLine("Сохранение недоступно: логгер не поддерживает запись.");
                return;
            }

            Console.Write("Введите название сохранения: ");
            string saveName = (Console.ReadLine() ?? "").Trim();

            var save = BattleSaveService.Instance.CreateInProgressSave(
                army1,
                army2,
                turns,
                army1Turn,
                _scoreArmy1,
                _scoreArmy2,
                rec.Lines,
                saveName);

            string fileName = BattleSaveService.Instance.Save(save, saveName);
            Console.WriteLine($"Игра сохранена: {fileName}");
        }

        private void PrintArmyState(IArmy army1, IArmy army2)
        {
            Console.WriteLine();
            Console.WriteLine($"Состав армии {army1.Name}:");
            foreach (var unit in army1.Units)
                Console.WriteLine($"  {unit}");

            Console.WriteLine();
            Console.WriteLine($"Состав армии {army2.Name}:");
            foreach (var unit in army2.Units)
                Console.WriteLine($"  {unit}");

            Console.WriteLine();
        }
    }
}