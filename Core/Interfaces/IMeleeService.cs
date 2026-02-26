using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    public interface IMeleeService
    {
        int Execute(IArmy attackerArmy,
                     IArmy defenderArmy,
                     bool attackerIsArmy1);
    }
}