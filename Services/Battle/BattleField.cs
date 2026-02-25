using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleLogger _logger;
        private readonly IRandomService _random;

        public BattleField(
            IDamageCalculator damageCalculator,
            IBattleLogger logger,
            IRandomService random)
        {
            _damageCalculator = damageCalculator;
            _logger = logger;
            _random = random;
        }

        public BattleResult StartBattle(IArmy army1, IArmy army2)
        {
            int turns = 0;
            bool isArmy1Turn = _random.Next(0, 2) == 0;

            _logger.Log($"Battle started: {army1.Name} vs {army2.Name}");
            _logger.Log($"First turn: {(isArmy1Turn ? army1.Name : army2.Name)}");

            while (army1.HasAliveUnits && army2.HasAliveUnits)
            {
                turns++;

                var attackerArmy = isArmy1Turn ? army1 : army2;
                var defenderArmy = isArmy1Turn ? army2 : army1;

                MakeTurn(attackerArmy, defenderArmy);

                isArmy1Turn = !isArmy1Turn;
            }

            var winner = army1.HasAliveUnits ? army1.Name : army2.Name;
            _logger.Log($"Battle finished in {turns} turns. Winner: {winner}");

            return new BattleResult(winner, turns);
        }

        private void MakeTurn(IArmy attackingArmy, IArmy defendingArmy)
        {
            var attacker = attackingArmy.GetCurrentAliveUnit();
            var defender = defendingArmy.GetCurrentAliveUnit();

            // Логируем начало хода
            _logger.LogInfo($"--- {attackingArmy.Name} ход ---");

            if (attacker.SpecialAbility != null && attacker.SpecialAbility.CanUse(attacker))
            {
                int damage = _damageCalculator.CalculateDamage(attacker, defender);
                defender.TakeDamage(damage);

                _logger.LogSpecial(attacker, defender, attacker.SpecialAbility.Name, damage);
            }
            else
            {
                int damage = _damageCalculator.CalculateDamage(attacker, defender);
                defender.TakeDamage(damage);

                _logger.LogHit(attacker, defender, damage);
            }

            // Проверка смерти
            if (!defender.IsAlive)
            {
                _logger.LogDeath(defender);
                defendingArmy.MoveToNextAliveUnit();
            }

            if (!attacker.IsAlive)
            {
                _logger.LogDeath(attacker);
                attackingArmy.MoveToNextAliveUnit();
            }
        }
    }
}