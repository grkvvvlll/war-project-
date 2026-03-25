using Core.Interfaces;

namespace Core.Entities.Units
{
    public class LightUnit : Unit, ICanBeHealed, ICanBeCloned
    {
        public LightUnit(
            string name,
            int attack,
            int defence,
            int health,
            int cost)
            : base(name, attack, defence, health, cost)
        {
            SpecialAbility = null;
        }

        public LightUnit(
            string name,
            int attack,
            int defence,
            int health,
            int maxHealth,
            int cost)
            : base(name, attack, defence, health, maxHealth, cost)
        {
            SpecialAbility = null;
        }

        public IUnit Clone(IRandomService random)
        {
            return new LightUnit(
                Name + " (клон)",
                Attack,
                Defence,
                Health,      // Текущее HP
                MaxHealth,
                Cost);
        }
    }
}