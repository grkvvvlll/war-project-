using Core.Entities;
using Core.Interfaces;
using Core.Formations;
using Services.Battle;
using Services.Random;

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

        public int Score1 => _score1;
        public int Score2 => _score2;
        public int Round => _round;
        public bool IsOver => !HasAlive(_army1) || !HasAlive(_army2);

        public WpfBattleEngine(IArmy army1, IArmy army2, IBattleFormation formation)
        {
            _army1 = army1;
            _army2 = army2;
            _formation = formation;
            _random = new RandomService();
            _logger = new WpfBattleLogger();

            var damageCalculator = new DamageCalculator();
            _meleeService = new MeleeService(damageCalculator, _logger, formation);
            _specialAbilityService = new SpecialAbilityService(_logger, formation);

            _army1Turn = _random.Next(0, 2) == 0;
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

        private bool HasAlive(IArmy army) => army.Units.Any(u => u.IsAlive);
    }
}