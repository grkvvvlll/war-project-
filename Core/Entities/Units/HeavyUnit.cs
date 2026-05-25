namespace Core.Entities.Units
{
    public class HeavyUnit : Unit
    {
        public HeavyUnit(
            string name,
            int attack,
            int defence,
            int health,
            int cost)
            : base(name, attack, defence, health, cost)
        {
            SpecialAbility = null;
        }

        public HeavyUnit(
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
    }
}