using Core.Interfaces;
using MedievalRussia;

namespace Core.Entities.Units
{
    public class GulyayGorodAdapter : Unit, ICanNotBeCloned, ICanNotBeHealed
    {
        private readonly GulyayGorod _original;

        public GulyayGorodAdapter(
            string name,
            int health,
            int defence,
            int cost,
            GulyayGorod original)
            : base(name, 0, defence, health, cost)
        {
            _original = original;
            SpecialAbility = null; // У Гуляй-города нет способности
        }

        public override void TakeDamage(int damage)
        {
            if (damage < 0) return;

            // Берём защиту из оригинального GulyayGorod
            int reduced = damage - _original.Defence;
            if (reduced < 0) reduced = 0;

            _original.ReduceHealth(reduced);

            // Обновляем здоровье адаптера
            Health = _original.HasDestroyed ? 0 : _original.GetHealth();
        }
        public override bool IsAlive => !_original.HasDestroyed;
    }
}