using Core.Interfaces;
using Core.Entities.Abilities;

namespace Core.Entities.Units
{
    // не лечится сам, но может лечить других
    public class Healer : Unit, ICanBeCloned
    {
        public int HealRange { get; }
        public int HealPower { get; }

        public Healer(
            string name,
            int attack,
            int defence,
            int health,
            int cost,
            int healRange,
            int healPower)
            : base(name, attack, defence, health, cost)
        {
            HealRange = healRange;
            HealPower = healPower;
            SpecialAbility = new HealAbility(healRange, healPower);
        }

        public IUnit Clone(IRandomService random)
        {
            return new Healer(Name, Attack, Defence, MaxHealth, Cost, HealRange, HealPower);
        }
    }
}