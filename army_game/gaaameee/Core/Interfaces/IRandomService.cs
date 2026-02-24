namespace gaaameee.Core.Interfaces
{
    // Генерация случайных чисел и булевых значений.
    public interface IRandomService
    {
        /// <summary>
        /// Случайное целое число в диапазоне [min, max).
        /// </summary>
        /// <param name="min">Минимальное значение (включительно).</param>
        /// <param name="max">Максимальное значение (не вкл).</param>
        /// <returns>Случайное число от min до max-1.</returns>
        int Next(int min, int max);

        /// <summary>
        /// Случайное булевое значение (true/false).
        /// </summary>
        /// <returns>true или false.</returns>
        bool NextBool();
    }
}