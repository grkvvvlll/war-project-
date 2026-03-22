using Core.Interfaces;
using Core.Entities.Abilities;

namespace Core.Entities.Units
{
    public class Archer : Unit, ICanBeHealed, ICanBeCloned
    {
        public int Range { get; }

        public Archer(
            string name,
            int attack,
            int defence,
            int health,
            int cost,
            int range)
            : base(name, attack, defence, health, cost)
        {
            Range = range;
            SpecialAbility = new ArcherShotAbility(range);
        }

        public Archer(
            string name,
            int attack,
            int defence,
            int health,
            int maxHealth,
            int cost,
            int range)
            : base(name, attack, defence, health, maxHealth, cost)
        {
            Range = range;
            SpecialAbility = new ArcherShotAbility(range);
        }

        public IUnit Clone(IRandomService random)
        {
            return new Archer(Name, Attack, Defence, MaxHealth, MaxHealth, Cost, Range);
        }
    }
}