using Core.Interfaces;

namespace Core.Interfaces
{
    public interface IMeleeService
    {
        void SetFormation(IBattleFormation formation);
        int Execute(IArmy attackerArmy,
                     IArmy defenderArmy,
                     bool attackerIsArmy1);
    }
}