using Core.Interfaces;
using Core.Entities.Abilities;

namespace Core.Entities.Units
{
    // не лечится, но клонируется
    public class Wizard : Unit, ICanBeCloned
    {
        public int SpellRange { get; }
        public int ClonePower { get; }

        public Wizard(
            string name,
            int attack,
            int defence,
            int health,
            int cost,
            int spellRange,
            int clonePower,
            IRandomService random)
            : base(name, attack, defence, health, cost)
        {
            SpellRange = spellRange;
            ClonePower = clonePower;
            SpecialAbility = new CloneAbility(spellRange, clonePower, random);
        }

        public IUnit Clone(IRandomService random)
        {
            return new Wizard(Name, Attack, Defence, MaxHealth, Cost, SpellRange, ClonePower, random);
        }
    }
}