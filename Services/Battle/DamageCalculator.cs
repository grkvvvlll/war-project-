using gaaameee.Core.Interfaces;

namespace Services.Battle
{
    public class DamageCalculator : IDamageCalculator
    {
        public int CalculateDamage(IUnit attacker, IUnit defender)
        {
            int raw = attacker.Attack - defender.Defence;
            return raw > 0 ? raw : 0;
        }
    }
}