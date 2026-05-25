namespace Core.Interfaces
{
    // Контекст выполнения способности
    public interface IAbilityExecutionContext
    {
        IBattleFormation Formation { get; }
        IBattleLogger Logger { get; }
        IRandomService Random { get; }

        /// <summary>Дистанция от юнита армии до врага.</summary>
        int GetEnemyDistance(IArmy myArmy, int myIndex,
                             IArmy enemyArmy, int enemyIndex, bool isArmy1);

        /// <summary>Дистанция между союзниками в армии.</summary>
        int GetAllyDistance(IArmy army, int index1, int index2, bool isArmy1);

        /// <summary>Индексы соседей юнита в радиусе maxDist.</summary>
        List<int> GetNeighborIndices(IArmy army, int unitIndex, bool isArmy1, int maxDist);

        /// <summary>
        /// Вызывается способностью при создании нового юнита (клон).
        /// Сервис прикрепляет наблюдателей и помечает юнит как уже обработанный в этом раунде.
        /// </summary>
        void RegisterNewUnit(IUnit unit);
    }
}