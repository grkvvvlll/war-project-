using System.Linq;
using gaaameee.Core.Interfaces;

namespace Services.Battle
{
    public class MeleeService : IMeleeService
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleLogger _logger;

        public MeleeService(
            IDamageCalculator damageCalculator,
            IBattleLogger logger)
        {
            _damageCalculator = damageCalculator;
            _logger = logger;
        }

        public int Execute(
            IArmy attackerArmy,
            IArmy defenderArmy,
            bool attackerIsArmy1)
        {
            var attacker = attackerIsArmy1
                ? attackerArmy.Units.LastOrDefault(u => u.IsAlive)
                : attackerArmy.Units.FirstOrDefault(u => u.IsAlive);

            var defender = attackerIsArmy1
                ? defenderArmy.Units.FirstOrDefault(u => u.IsAlive)
                : defenderArmy.Units.LastOrDefault(u => u.IsAlive);

            if (attacker == null || defender == null)
                return 0;

            int damage =
                _damageCalculator.CalculateDamage(attacker, defender);

            int oldHp = defender.Health;

            defender.TakeDamage(damage);

            _logger.LogHit(
                attacker,
                defender,
                damage,
                oldHp,
                attackerIsArmy1);

            if (!defender.IsAlive)
            {
                _logger.LogDeath(defender, !attackerIsArmy1);
                return defender.Cost;   
            }

            return 0;
        }
    }
}