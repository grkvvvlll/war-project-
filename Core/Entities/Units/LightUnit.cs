using Core.Interfaces;

namespace Core.Entities.Units
{
    // может быть исцеленным и клонированным
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

        public IUnit Clone(IRandomService random)
        {
            return new LightUnit(Name, Attack, Defence, MaxHealth, Cost);
        }
    }
}