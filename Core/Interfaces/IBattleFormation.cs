namespace Core.Interfaces
{
    public interface IBattleFormation
    {
        string Name { get; }
        string Description { get; }

        // Получить атакующего в ближнем бою
        IUnit? GetMeleeAttacker(IArmy attackerArmy, bool attackerIsArmy1);

        // Получить защитника в ближнем бою
        IUnit? GetMeleeDefender(IArmy defenderArmy, bool attackerIsArmy1);

        // Стоит ли юнит на фронтовой позиции (строка 0)
        bool IsOnFrontLine(IArmy army, int unitIndex, bool isArmy1);

        // Получить дистанцию между юнитом своей армии и юнитом вражеской (чебышёв + зазор)
        int GetDistanceBetweenUnits(IArmy myArmy, int myIndex, IArmy enemyArmy, int enemyIndex, bool isArmy1);

        // Получить дистанцию между двумя юнитами одной армии (чебышёв, без зазора)
        int GetDistanceBetweenAllies(IArmy army, int index1, int index2, bool isArmy1);

        // Может ли юнит использовать special ability (не на фронте — или фронт, но напротив пусто)
        bool CanUseSpecialAbility(IArmy myArmy, int unitIndex, IArmy enemyArmy, bool isArmy1);

        // Для ближнего боя: список пар (attackerIndex, defenderIndex) на этот раунд
        List<(int attackerIndex, int defenderIndex)> GetMeleePairs(IArmy attackerArmy, IArmy defenderArmy, bool attackerIsArmy1);

        // Получить (строку, столбец) юнита
        (int row, int col) GetPosition(IArmy army, int unitIndex, bool isArmy1);
    }
}
