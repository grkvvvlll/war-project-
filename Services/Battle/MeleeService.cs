using Core.Entities;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Interfaces;

namespace Services.Battle
{
    public class MeleeService : IMeleeService
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleLogger _logger;
        private IBattleFormation _formation;

        public MeleeService(
            IDamageCalculator damageCalculator,
            IBattleLogger logger,
            IBattleFormation formation)
        {
            _damageCalculator = damageCalculator;
            _logger = logger;
            _formation = formation;
        }

        public void SetFormation(IBattleFormation formation) => _formation = formation;

        public int Execute(
            IArmy attackerArmy,
            IArmy defenderArmy,
            bool attackerIsArmy1)
        {
            int totalScore = 0;

            var pairs = _formation.GetMeleePairs(attackerArmy, defenderArmy, attackerIsArmy1);

            foreach (var (aIdx, dIdx) in pairs)
            {
                var attacker = attackerArmy.Units[aIdx];
                var defender = defenderArmy.Units[dIdx];

                if (!attacker.IsAlive || !defender.IsAlive) continue;
                if (attacker is GulyayGorodAdapter) continue;

                int damage = _damageCalculator.CalculateDamage(attacker, defender);
                int oldHp = defender.Health;

                defender.TakeDamage(damage);

                _logger.LogHit(attacker, defender, damage, oldHp, attackerIsArmy1);

                // Сразу показываем потерю баффа если слетел
                if (defender is UnitDecorator dec && dec.BrokenBuff != null)
                {
                    string unitName = dec.GetInnerUnit().Name;
                    string buffName = dec.BrokenBuff.NameNominative;
                    Console.ForegroundColor = attackerIsArmy1 ? ConsoleColor.Red : ConsoleColor.White;
                    Console.Write($"{unitName} ");
                    Console.ResetColor();
                    Console.WriteLine($"💥 потерял бафф {buffName}!");
                    ((Core.Entities.Army)defenderArmy).SetUnit(dIdx, dec.GetInnerUnit());
                }

                if (!defender.IsAlive)
                {
                    _logger.LogDeath(defender, !attackerIsArmy1);
                    totalScore += defender.Cost;
                }
            }

            return totalScore;
        }
    }
}