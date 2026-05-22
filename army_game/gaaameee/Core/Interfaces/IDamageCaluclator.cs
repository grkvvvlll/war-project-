using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    public interface IDamageCalculator
    {
        /// <summary>
        /// Рассчитать урон, который атакующий юнит наносит защитнику.
        /// </summary>
        /// <param name="attacker">Атакующий юнит.</param>
        /// <param name="defender">Защитник.</param>
        /// <returns>Количество нанесённого урона (не может быть меньше 0).</returns>
        int Calculate(IUnit attacker, IUnit defender);
    }
}