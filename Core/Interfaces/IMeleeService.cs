using Core.Interfaces;

namespace Core.Interfaces
{
    public interface IMeleeService
    {
        int Execute(IArmy attackerArmy,
                     IArmy defenderArmy,
                     bool attackerIsArmy1);
    }
}