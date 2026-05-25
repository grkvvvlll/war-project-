using Core.Entities;
using Core.Entities.Units;
using Core.Interfaces;
using Services.Storage;
using Services.Commands;
using Services.Observers;
using Services.Logging;
using Core.Formations;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        public const string SavedAndStoppedResult = "Бой сохранён и остановлен";
        public const string StoppedWithoutSaveResult = "Бой остановлен без сохранения";
        public const string DrawResult = "Ничья";

        private readonly IMeleeService _meleeService;
        private readonly ISpecialAbilityService _specialAbilityService;
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;
        private readonly BattleSaveService _saveService;
        private readonly IBattleUI _ui;
        private IBattleFormation _formation;

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
            ISpecialAbilityService specialAbilityService,
            IRandomService random,
            IBattleLogger logger,
            IBattleFormation formation,
            BattleSaveService saveService,
            IBattleUI ui)
        {
            _meleeService = meleeService;
            _specialAbilityService = specialAbilityService;
            _random = random;
            _logger = logger;
            _formation = formation;
            _saveService = saveService;
            _ui = ui;
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
            var history = new CommandHistory();
            var snapshotService = new BattleStateSnapshotService(_random, _saveService);
            var initialSnapshot = CaptureSnapshot("Начальное состояние");

            BattleSave CaptureSnapshot(string description)
            {
                var logLines = _logger is IRecordingBattleLogger rec
                    ? rec.Lines
                    : Enumerable.Empty<string>();

                return snapshotService.Capture(
                    army1,
                    army2,
                    turns,
                    currentArmy1Turn,
                    _scoreArmy1,
                    _scoreArmy2,
                    _formation,
                    logLines,
                    description);
            }

            void ApplySnapshot(BattleSave snapshot)
            {
                ObserverRegistry.Instance.Detach(army1);
                ObserverRegistry.Instance.Detach(army2);

                var restored = snapshotService.Restore(snapshot);
                army1 = restored.Army1;
                army2 = restored.Army2;
                turns = restored.Turns;
                currentArmy1Turn = restored.Army1Turn;
                _scoreArmy1 = restored.ScoreArmy1;
                _scoreArmy2 = restored.ScoreArmy2;
                SetFormation(restored.Formation);

                if (_logger is RecordingBattleLogger rec)
                {
                    rec.Clear();
                    foreach (var line in snapshot.LogLines)
                        rec.Lines.Add(line);
                }

                ObserverRegistry.Instance.Attach(army1);
                ObserverRegistry.Instance.Attach(army2);
            }

            void ResetToInitialState()
            {
                ApplySnapshot(initialSnapshot);
                history.Clear();
                _logger.LogInfo("Бой возвращён к начальному состоянию.");
            }

            void ExecuteFormationCommand()
            {
                var before = CaptureSnapshot("Перед изменением построения");
                BattleSave? after = null;

                history.Execute(new ActionGameCommand(
                    "Изменение построения",
                    execute: () =>
                    {
                        if (after != null)
                        {
                            ApplySnapshot(after);
                            return;
                        }

                        ChangeFormation(army1, army2);
                        after = CaptureSnapshot("После изменения построения");
                    },
                    undo: () => ApplySnapshot(before)));
            }

            void ExecuteCurrentRound()
            {
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
                RenumberArmy(army1, isArmy1: true);
                RenumberArmy(army2, isArmy1: false);
                turns++;

                currentArmy1Turn = !currentArmy1Turn;
            }

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
                if (!WaitForRoundAction(
                    ref army1,
                    ref army2,
                    ref turns,
                    ref currentArmy1Turn,
                    history,
                    ApplySnapshot,
                    ResetToInitialState,
                    ExecuteFormationCommand))
                    return new BattleResult(
                        _exitWithoutSave ? StoppedWithoutSaveResult : SavedAndStoppedResult,
                        turns);
            }

            while (HasAlive(army1) && HasAlive(army2))
            {
                BattleVisualizer.PrintArmyLine(army1, army2, _formation);
                _ui.PrintMessage("");

                // проверяем, остались ли только Гуляй-города
                if (IsOnlyGulyayGorodVsGulyayGorod(army1, army2))
                {
                    _logger.LogInfo("Остались только крепости с обеих сторон!");
                }

                var before = CaptureSnapshot($"Перед ходом {turns + 1}");
                BattleSave? after = null;
                var roundNumber = turns + 1;

                history.Execute(new ActionGameCommand(
                    $"Ход {roundNumber}",
                    execute: () =>
                    {
                        if (after != null)
                        {
                            ApplySnapshot(after);
                            return;
                        }

                        ExecuteCurrentRound();
                        after = CaptureSnapshot($"После хода {turns}");
                    },
                    undo: () => ApplySnapshot(before)));

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

                if (!_autoMode && HasAlive(army1) && HasAlive(army2))
                {
                    if (!WaitForRoundAction(
                        ref army1,
                        ref army2,
                        ref turns,
                        ref currentArmy1Turn,
                        history,
                        ApplySnapshot,
                        ResetToInitialState,
                        ExecuteFormationCommand))
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

        private bool WaitForRoundAction(
            ref IArmy army1,
            ref IArmy army2,
            ref int turns,
            ref bool army1Turn,
            CommandHistory history,
            Action<BattleSave> applySnapshot,
            Action resetToInitialState,
            Action changeFormationCommand)
        {
            while (true)
            {
                var choice = _ui.WaitForChoice(turns + 1);
                switch (choice)
                {
                    case RoundMenuChoice.NextRound:
                        return true;
                    case RoundMenuChoice.ShowArmyState:
                        _ui.PrintArmyState(army1, army2); continue;
                    case RoundMenuChoice.SaveAndExit:
                        SaveBattle(army1, army2, turns, army1Turn); return false;
                    case RoundMenuChoice.AutoMode:
                        _autoMode = true; return true;
                    case RoundMenuChoice.ExitWithoutSave:
                        _exitWithoutSave = true; return false;
                    case RoundMenuChoice.ChangeFormation:
                        changeFormationCommand(); continue;
                    case RoundMenuChoice.Undo:
                        if (!history.CanUndo) _ui.PrintMessage("Undo недоступен.");
                        else history.Undo();
                        continue;
                    case RoundMenuChoice.Redo:
                        if (!history.CanRedo) _ui.PrintMessage("Redo недоступен.");
                        else history.Redo();
                        continue;
                    case RoundMenuChoice.Reset:
                        resetToInitialState(); continue;
                    case RoundMenuChoice.ShowHistory:
                        _ui.PrintHistory(history.Entries); continue;
                    default:
                        _ui.PrintMessage("Неизвестная команда."); continue;
                }
            }
        }
        private void SaveBattle(IArmy army1, IArmy army2, int turns, bool army1Turn)
        {
            if (_logger is not IRecordingBattleLogger rec)
            {
                _ui.PrintSaveFailed();
                return;
            }
            string saveName = _ui.ReadSaveName();
            var save = _saveService.CreateInProgressSave(
                army1, army2, turns, army1Turn,
                _scoreArmy1, _scoreArmy2, _formation, rec.Lines, saveName);
            string fileName = _saveService.Save(save, saveName);
            _ui.PrintSaved(fileName);
        }

        public void SetFormation(IBattleFormation formation)
        {
            _formation = formation;
            _meleeService.SetFormation(formation);
            _specialAbilityService.SetFormation(formation);
        }

        private void ChangeFormation(IArmy army1, IArmy army2)
        {
            var previousFormation = _formation;
            var newFormation = _ui.ReadFormationChoice();
            if (newFormation == null) { _ui.PrintMessage("Неверный ввод."); return; }

            SetFormation(newFormation);

            bool wasWall = previousFormation is WallFormation;
            bool isWall = _formation is WallFormation;
            if (wasWall != isWall)
                army1.ReverseUnits();

            RenumberArmy(army1, isArmy1: true);
            RenumberArmy(army2, isArmy1: false);
        }
        private void RenumberArmy(IArmy army, bool isArmy1)
        {
            var alive = army.Units.Where(u => u.IsAlive).ToList();
            bool isWall = _formation is WallFormation;

            if (isArmy1 && !isWall)
            {
                for (int i = 0; i < alive.Count; i++)
                {
                    var unit = alive[alive.Count - 1 - i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
            else
            {
                for (int i = 0; i < alive.Count; i++)
                {
                    var unit = alive[i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
        }

        public IBattleFormation GetFormation() => _formation;
    }
}