using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    public interface IUnit
    {
        // Сила атаки.
        int Attack { get; }

        // Защита юнита.
        int Defence { get; }

        // Текущее здоровье.
        int Health { get; }

        // Стоимость юнита (награда за убийство)
        int Cost { get; }

        // Жив ли юнит.
        bool IsAlive { get; }

        // Специальная способность (может быть null).
        ISpecialAbility? SpecialAbility { get; }

        // Нанести базовый удар другому юниту 
        void Hit(IUnit target);

        // Получить урон.
        void TakeDamage(int damage);
    }
}