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
            SpecialAbility = null;
        }

        public override void TakeDamage(int damage)
        {
            if (damage < 0) return;

            _original.ReduceHealth(damage);

            Health = _original.HasDestroyed ? 0 : _original.GetHealth();
        }

        public override bool IsAlive => !_original.HasDestroyed;
    }
}