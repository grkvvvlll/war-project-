namespace gaaameee.Core.Interfaces
{
    public interface IDamageCalculator
    {
        int CalculateDamage(IUnit attacker, IUnit defender);
    }
}