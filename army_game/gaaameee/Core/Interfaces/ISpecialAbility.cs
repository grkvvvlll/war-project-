using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    // Спецспособности юнита.
    public interface ISpecialAbility
    {
        /// <summary>
        /// Название способности (например, "Стрела").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Дальность применения способности.
        /// </summary>
        int Range { get; }

        /// <summary>
        /// Сила урона способности.
        /// </summary>
        int Power { get; }

        /// <summary>
        /// Проверяет, может ли способность быть применена к цели.
        /// </summary>
        /// <param name="attacker">Юнит, использующий способность.</param>
        /// <param name="target">Цель.</param>
        /// <returns>true, если цель в пределах действия способности.</returns>
        bool CanUse(IUnit attacker, IUnit target);

        /// <summary>
        /// Применяет способность к цели.
        /// </summary>
        /// <param name="attacker">Юнит, использующий способность.</param>
        /// <param name="target">Цель (может быть null, если способность массовая).</param>
        /// <param name="random">Сервис для рандома.</param>
        /// <returns>Урон, который был нанесён.</returns>
        int Use(IUnit attacker, IUnit? target, IRandomService random);
    }
}