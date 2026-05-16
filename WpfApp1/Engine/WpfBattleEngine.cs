using Core.Entities;
using Core.Interfaces;
using Core.Formations;
using Services.Battle;
using Services.Commands;
using Services.Observers;
using Services.Random;
using Services.Storage;

namespace WpfPresentation.Engine
{
    public class WpfBattleEngine
    {
        private IArmy _army1;
        private IArmy _army2;
        private IBattleFormation _formation;
        private readonly IRandomService _random;
        private readonly WpfBattleLogger _logger;
        private MeleeService _meleeService;
        private SpecialAbilityService _specialAbilityService;

        private int _score1 = 0;
        private int _score2 = 0;
        private int _round = 0;
        private bool _army1Turn;
        private readonly BattleStateSnapshotService _snapshotService;
        private readonly BattleSave _initialSnapshot;
        private List<BattleEvent> _lastEvents = new();

        public int Score1 => _score1;
        public int Score2 => _score2;
        public int Round => _round;
        public bool IsOver => !HasAlive(_army1) || !HasAlive(_army2);
        public CommandHistory History { get; } = new();

        // Exposing state for save/load
        public IArmy Army1 => _army1;
        public IArmy Army2 => _army2;
        public bool Army1TurnState => _army1Turn;
        public IBattleFormation Formation => _formation;

        public WpfBattleEngine(IArmy army1, IArmy army2, IBattleFormation formation)
        {
            _army1 = army1;
            _army2 = army2;
            _formation = formation;
            _random = new RandomService();
            _logger = new WpfBattleLogger();
            _logger.SetArmies(_army1, _army2);   // ← индексы для анимаций

            var damageCalculator = new DamageCalculator();
            _meleeService = new MeleeService(damageCalculator, _logger, formation);
            _specialAbilityService = new SpecialAbilityService(_logger, formation);

            _army1Turn = _random.Next(0, 2) == 0;
            _snapshotService = new BattleStateSnapshotService(_random);
            _initialSnapshot = CaptureSnapshot("Начальное состояние");
            AttachDeathObserver();
        }

        // Resume from a saved game
        public WpfBattleEngine(BattleResumeData resume)
        {
            _army1 = resume.Army1;
            _army2 = resume.Army2;
            _formation = resume.Formation;
            _random = new RandomService();
            _logger = new WpfBattleLogger();
            _logger.SetArmies(_army1, _army2);   // ← индексы для анимаций

            var damageCalculator = new DamageCalculator();
            _meleeService = new MeleeService(damageCalculator, _logger, _formation);
            _specialAbilityService = new SpecialAbilityService(_logger, _formation);

            _round = resume.Turns;
            _army1Turn = resume.Army1Turn;
            _score1 = resume.ScoreArmy1;
            _score2 = resume.ScoreArmy2;
            _snapshotService = new BattleStateSnapshotService(_random);
            _initialSnapshot = CaptureSnapshot("Начальное состояние");
            AttachDeathObserver();
        }

        public void SetFormation(IBattleFormation formation)
        {
            _formation = formation;
            _meleeService.SetFormation(formation);
            _specialAbilityService.SetFormation(formation);
        }

        public List<BattleEvent> ExecuteRound()
        {
            _logger.Events.Clear();
            _round++;

            if (_army1Turn)
            {
                _score1 += _meleeService.Execute(_army1, _army2, true);
                if (HasAlive(_army1) && HasAlive(_army2))
                    _score2 += _meleeService.Execute(_army2, _army1, false);
                if (HasAlive(_army1) && HasAlive(_army2))
                    _score1 += _specialAbilityService.Execute(_army1, _army2, true);
                if (HasAlive(_army1) && HasAlive(_army2))
                    _score2 += _specialAbilityService.Execute(_army2, _army1, false);
            }
            else
            {
                _score2 += _meleeService.Execute(_army2, _army1, false);
                if (HasAlive(_army1) && HasAlive(_army2))
                    _score1 += _meleeService.Execute(_army1, _army2, true);
                if (HasAlive(_army1) && HasAlive(_army2))
                    _score2 += _specialAbilityService.Execute(_army2, _army1, false);
                if (HasAlive(_army1) && HasAlive(_army2))
                    _score1 += _specialAbilityService.Execute(_army1, _army2, true);
            }

            _army1.RemoveDeadUnits();
            _army2.RemoveDeadUnits();

            _army1Turn = !_army1Turn;

            _logger.Events.Add(new BattleEvent
            {
                Type = BattleEventType.RoundEnd,
                Score1 = _score1,
                Score2 = _score2,
                Round = _round
            });

            if (!HasAlive(_army1) || !HasAlive(_army2))
            {
                string winner = HasAlive(_army1) ? _army1.Name : _army2.Name;
                if (!HasAlive(_army1) && !HasAlive(_army2)) winner = "Ничья";
                _logger.Events.Add(new BattleEvent
                {
                    Type = BattleEventType.BattleEnd,
                    Winner = winner,
                    Score1 = _score1,
                    Score2 = _score2,
                    Round = _round
                });
            }

            return _logger.Events.ToList();
        }

        public List<BattleEvent> ExecuteRoundCommand()
        {
            _lastEvents = new List<BattleEvent>();
            var before = CaptureSnapshot($"Перед ходом {_round + 1}");
            BattleSave? after = null;

            var command = new ActionGameCommand(
                $"Ход {_round + 1}",
                execute: () =>
                {
                    if (after != null)
                    {
                        ApplySnapshot(after);
                        _lastEvents = new List<BattleEvent>();
                        return;
                    }

                    _lastEvents = ExecuteRound();
                    after = CaptureSnapshot($"После хода {_round}");
                },
                undo: () => ApplySnapshot(before));

            History.Execute(command);
            return _lastEvents.ToList();
        }

        public void ChangeFormationCommand(IBattleFormation formation)
        {
            var before = CaptureSnapshot("Перед сменой построения");
            BattleSave? after = null;

            History.Execute(new ActionGameCommand(
                $"Построение: {GetFormationName(formation)}",
                execute: () =>
                {
                    if (after != null)
                    {
                        ApplySnapshot(after);
                        return;
                    }

                    ChangeFormation(formation);
                    after = CaptureSnapshot("После смены построения");
                },
                undo: () => ApplySnapshot(before)));
        }

        public void UndoCommand() => History.Undo();

        public void RedoCommand() => History.Redo();

        public void ResetToInitialStateCommand()
        {
            ApplySnapshot(_initialSnapshot);
            History.Clear();
        }

        private BattleSave CaptureSnapshot(string description)
        {
            return _snapshotService.Capture(
                _army1,
                _army2,
                _round,
                _army1Turn,
                _score1,
                _score2,
                _formation,
                Array.Empty<string>(),
                description);
        }

        private void ApplySnapshot(BattleSave snapshot)
        {
            DetachDeathObserver();
            var restored = _snapshotService.Restore(snapshot);
            _army1 = restored.Army1;
            _army2 = restored.Army2;
            _round = restored.Turns;
            _army1Turn = restored.Army1Turn;
            _score1 = restored.ScoreArmy1;
            _score2 = restored.ScoreArmy2;
            SetFormation(restored.Formation);
            _logger.SetArmies(_army1, _army2);
            AttachDeathObserver();
        }

        private static string GetFormationName(IBattleFormation formation)
        {
            return formation switch
            {
                WideBridgeFormation => "широкий мост",
                WallFormation => "стенка на стенку",
                _ => "мост"
            };
        }

        private void ChangeFormation(IBattleFormation formation)
        {
            bool wasWall = _formation is WallFormation;
            bool isWall = formation is WallFormation;

            SetFormation(formation);

            if (wasWall != isWall && _army1 is Army army)
                army.ReverseUnits();

            RenumberArmy(_army1, isArmy1: true);
            RenumberArmy(_army2, isArmy1: false);
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

        private void AttachDeathObserver()
        {
            foreach (var unit in _army1.Units)
                ObserverRegistry.DeathObserver.Subscribe(unit);
            foreach (var unit in _army2.Units)
                ObserverRegistry.DeathObserver.Subscribe(unit);
        }

        private void DetachDeathObserver()
        {
            foreach (var unit in _army1.Units)
                ObserverRegistry.DeathObserver.Unsubscribe(unit);
            foreach (var unit in _army2.Units)
                ObserverRegistry.DeathObserver.Unsubscribe(unit);
        }

        private bool HasAlive(IArmy army) => army.Units.Any(u => u.IsAlive);
    }
}
