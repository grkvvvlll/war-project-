using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Entities
{
    public class ArcherShotAbility : ISpecialAbility
    {
        private readonly int _range;

        //  Название способности (для логирования)
        public string Name => "Выстрел стрелой";

        public ArcherShotAbility(int range)
        {
            _range = range;
        }

        // Проверка, может ли юнит использовать способность
        // Пока что единственное условие — юнит жив
        public bool CanUse(IUnit user)
        {
            return user.IsAlive;
        }

        // Применение способности к цели. Урон по формуле: damage = attack - defence

        public void Use(IUnit user, IUnit target)
        {
            // Если цель мертва — ничего не делаем
            if (!target.IsAlive)
                return;

            int damage = user.Attack - target.Defence;

            // Урон не может быть отрицательным
            if (damage < 0)
                damage = 0;

            // Применяем урон к цели
            target.TakeDamage(damage);
        }
    }
}