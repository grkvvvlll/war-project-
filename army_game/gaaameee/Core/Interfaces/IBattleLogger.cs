using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    //  Логирование событий боя
    public interface IBattleLogger
    {
        /// <summary>
        /// Лог простого сообщения.
        /// </summary>
        /// <param name="message">Сообщение для логирования.</param>
        void Log(string message);

        /// <summary>
        /// Лог удара одного юнита по другому.
        /// </summary>
        /// <param name="attacker">Атакующий юнит.</param>
        /// <param name="defender">Защищающийся юнит.</param>
        /// <param name="damage">Нанесенный урон.</param>
        void LogHit(IUnit attacker, IUnit defender, int damage);

        /// <summary>
        /// Лог смерти юнита.
        /// </summary>
        /// <param name="unit">Юнит, который погиб.</param>
        void LogDeath(IUnit unit);

        /// <summary>
        /// Лог использования спецспособности юнитом.
        /// </summary>
        /// <param name="unit">Юнит, использующий способность.</param>
        /// <param name="abilityName">Название способности.</param>
        /// <param name="target">Цель способности (может быть null).</param>
        /// <param name="damage">Урон, если применимо.</param>
        void LogSpecialAbility(IUnit unit, string abilityName, IUnit? target, int damage);
    }
}