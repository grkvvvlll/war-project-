namespace Core.Interfaces
{
    public interface ISpecialAbility
    {
        string Name { get; }
        string Description { get; }

        // Может ли юнит использовать способность 
        bool CanUse(IUnit user);

        // Может ли способность быть применена к цели
        bool CanTarget(IUnit user, IUnit target, bool isAlly);

        // Применение способности
        void Use(IUnit user, IUnit target, int distance);

        // Сброс накопленной вероятности для мага
        void ResetCharge();

        // Увеличение накопленной вероятности для мага
        void Charge();

        /// <summary>
        /// Полное выполнение способности: поиск цели, применение, логирование.
        /// Возвращает очки (стоимость убитых юнитов), 0 если никто не погиб.
        /// </summary>
        int Execute(IUnit user, int userIndex,
                    IArmy ownArmy, IArmy enemyArmy,
                    bool isArmy1, IAbilityExecutionContext ctx);
    }
}