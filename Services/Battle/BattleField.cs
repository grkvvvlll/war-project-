using System.Linq;
using System.Threading;
using Core.Entities;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Interfaces;
using Services.Storage;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        public const string SavedAndStoppedResult = "Бой сохранён и остановлен";
        public const string StoppedWithoutSaveResult = "Бой остановлен без сохранения";
        public const string DrawResult = "Ничья";  

        private readonly IMeleeService _meleeService;
        private readonly SpecialAbilityService _specialAbilityService;
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;

        private int _scoreArmy1 = 0;
        private int _scoreArmy2 = 0;
        private bool _autoMode = false;
        private bool _exitWithoutSave = false;

        // отслеживание состояния для ничьей
        private int _unchangedTurnsCount = 0;
        private int _previousTotalHp1 = 0;
        private int _previousTotalHp2 = 0;
        private int _previousAliveCount1 = 0;
        private int _previousAliveCount2 = 0;
        private const int MAX_UNCHANGED_TURNS = 10;  // лимит ходов без изменений

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
            int scoreArmy2 = 0,
            bool autoMode = false,
            bool showRoundMenuBeforeFirstRound = true)
        {
            _autoMode = autoMode;
            _scoreArmy1 = scoreArmy1;
            _scoreArmy2 = scoreArmy2;
            _exitWithoutSave = false;

            // сброс счётчиков при начале боя
            _unchangedTurnsCount = 0;
            _previousTotalHp1 = GetTotalHealth(army1);
            _previousTotalHp2 = GetTotalHealth(army2);
            _previousAliveCount1 = GetAliveCount(army1);
            _previousAliveCount2 = GetAliveCount(army2);

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

            if (!_autoMode && showRoundMenuBeforeFirstRound)
            {
                if (!WaitForRoundAction(army1, army2, turns, currentArmy1Turn))
                    return new BattleResult(
                        _exitWithoutSave ? StoppedWithoutSaveResult : SavedAndStoppedResult,
                        turns);
            }

            while (HasAlive(army1) && HasAlive(army2))
            {
                BattleVisualizer.PrintArmyLine(army1, army2);
                Console.WriteLine();

                // проверяем, остались ли только Гуляй-города
                if (IsOnlyGulyayGorodVsGulyayGorod(army1, army2))
                {
                    _logger.LogInfo("Остались только крепости с обеих сторон!");
                }

                if (currentArmy1Turn)
                {
                    _scoreArmy1 += _meleeService.Execute(army1, army2, true);
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _meleeService.Execute(army2, army1, false);
                    CleanAndLogBrokenBuffs(army1, true);
                    CleanAndLogBrokenBuffs(army2, false);
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
                    CleanAndLogBrokenBuffs(army2, false);
                    CleanAndLogBrokenBuffs(army1, true);
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _specialAbilityService.Execute(army2, army1, false);
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _specialAbilityService.Execute(army1, army2, true);
                }

                _logger.LogInfo($"СЧЁТ: {_scoreArmy1} : {_scoreArmy2}");

                army1.RemoveDeadUnits();
                army2.RemoveDeadUnits();
                CleanBrokenBuffs(army1);
                CleanBrokenBuffs(army2);
                turns++;

                // проверяем изменения состояния
                int currentTotalHp1 = GetTotalHealth(army1);
                int currentTotalHp2 = GetTotalHealth(army2);
                int currentAliveCount1 = GetAliveCount(army1);
                int currentAliveCount2 = GetAliveCount(army2);

                bool hasChanges =
                    currentTotalHp1 != _previousTotalHp1 ||
                    currentTotalHp2 != _previousTotalHp2 ||
                    currentAliveCount1 != _previousAliveCount1 ||
                    currentAliveCount2 != _previousAliveCount2;

                if (hasChanges)
                {
                    _unchangedTurnsCount = 0;  // сброс счётчика
                }
                else
                {
                    _unchangedTurnsCount++;  // увеличиваем счётчик
                    if (_unchangedTurnsCount >= MAX_UNCHANGED_TURNS)
                    {
                        _logger.LogInfo($" {MAX_UNCHANGED_TURNS} ходов без изменений — объявляется ничья");
                        return new BattleResult(DrawResult, turns);
                    }
                }

                // Сохраняем текущее состояние для следующего хода
                _previousTotalHp1 = currentTotalHp1;
                _previousTotalHp2 = currentTotalHp2;
                _previousAliveCount1 = currentAliveCount1;
                _previousAliveCount2 = currentAliveCount2;

                currentArmy1Turn = !currentArmy1Turn;

                if (!_autoMode && HasAlive(army1) && HasAlive(army2))
                {
                    if (!WaitForRoundAction(army1, army2, turns, currentArmy1Turn))
                        return new BattleResult(
                            _exitWithoutSave ? StoppedWithoutSaveResult : SavedAndStoppedResult,
                            turns);
                }

                if (_autoMode)
                {
                    Thread.Sleep(1000);
                }
            }

            string winner = HasAlive(army1) ? army1.Name : army2.Name;
            return new BattleResult(winner, turns);
        }

        private int GetTotalHealth(IArmy army)
        {
            return army.Units.Where(u => u.IsAlive).Sum(u => u.Health);
        }

        private int GetAliveCount(IArmy army)
        {
            return army.Units.Count(u => u.IsAlive);
        }

        private bool IsOnlyGulyayGorodVsGulyayGorod(IArmy army1, IArmy army2)
        {
            var alive1 = army1.Units.Where(u => u.IsAlive).ToList();
            var alive2 = army2.Units.Where(u => u.IsAlive).ToList();

            return alive1.Count == 1 && alive2.Count == 1 &&
                   alive1[0] is GulyayGorodAdapter &&
                   alive2[0] is GulyayGorodAdapter;
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
                Console.WriteLine("3 - проиграть до конца");
                Console.WriteLine("4 - выйти в меню без сохранения");
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
                if (input == "3")
                {
                    _autoMode = true;
                    return true;
                }
                if (input == "4")
                {
                    _exitWithoutSave = true;
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
            Thread.Sleep(30);
            foreach (var unit in army1.Units)
            {
                // Явное форматирование вместо неявного ToString()
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
            Thread.Sleep(30);
        }

        private void CleanBrokenBuffs(IArmy army)
        {
            for (int i = 0; i < army.Units.Count; i++)
            {
                if (army.Units[i] is UnitDecorator decorator && decorator.IsBroken())
                {
                    // Заменяем декоратор на внутренний юнит
                    ((Army)army).SetUnit(i, decorator.GetInnerUnit());
                    _logger.LogInfo($"{decorator.GetInnerUnit().Name} потерял экипировку!");
                }
            }
        }

        private void CleanAndLogBrokenBuffs(IArmy army, bool isArmy1)
        {
            // Проходим с конца, чтобы безопасно заменять элементы в списке
            for (int i = army.Units.Count - 1; i >= 0; i--)
            {
                if (army.Units[i] is Core.Entities.Buffs.UnitDecorator decorator && decorator.BrokenBuff != null)
                {
                    string buffName = decorator.BrokenBuff.NameNominative; // "Шлем", "Конь" и т.д.
                    string unitName = decorator.GetInnerUnit().Name;       // Имя юнита без этого баффа

                    Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                    Console.Write($"{unitName} ");
                    Console.ResetColor();
                    Console.WriteLine($"потерял {buffName}!");

                    // Снимаем декоратор: заменяем его в армии на "голый" юнит внутри
                    ((Army)army).SetUnit(i, decorator.GetInnerUnit());
                }
            }
        }
    }
}