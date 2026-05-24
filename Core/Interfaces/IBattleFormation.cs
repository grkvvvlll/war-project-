namespace Core.Interfaces
{
    public interface IBattleFormation
    {
        string Name { get; }
        string Description { get; }

        IUnit? GetMeleeAttacker(IArmy attackerArmy, bool attackerIsArmy1);

        IUnit? GetMeleeDefender(IArmy defenderArmy, bool attackerIsArmy1);

        bool IsOnFrontLine(IArmy army, int unitIndex, bool isArmy1);

        int GetDistanceBetweenUnits(IArmy myArmy, int myIndex, IArmy enemyArmy, int enemyIndex, bool isArmy1);

        int GetDistanceBetweenAllies(IArmy army, int index1, int index2, bool isArmy1);

        bool CanUseSpecialAbility(IArmy myArmy, int unitIndex, IArmy enemyArmy, bool isArmy1);

        List<(int attackerIndex, int defenderIndex)> GetMeleePairs(IArmy attackerArmy, IArmy defenderArmy, bool attackerIsArmy1);

        (int row, int col) GetPosition(IArmy army, int unitIndex, bool isArmy1);
    }
}
