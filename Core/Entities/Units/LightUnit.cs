using Core.Interfaces;
using Core.Entities.Abilities;

namespace Core.Entities.Units
{
    public class LightUnit : Unit, ICanBeHealed, ICanBeCloned
    {
        public LightUnit(string name, int attack, int defence, int health, int cost, IRandomService random) : base(name, attack, defence, health, cost)
        {
            SpecialAbility = new SquireAbility(random);
        }

        public LightUnit(
            string name,
            int attack,
            int defence,
            int health,
            int maxHealth,
            int cost,
            IRandomService random)
            : base(name, attack, defence, health, maxHealth, cost)
        {
            SpecialAbility = new SquireAbility(random);
        }

        public IUnit Clone(IRandomService random)
        {
            return new LightUnit(
                Name + " (клон)",
                Attack,
                Defence,
                Health,      // Текущее HP
                MaxHealth,
                Cost,
                random);
        }
    }
}
