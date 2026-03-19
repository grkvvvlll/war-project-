using Core.Interfaces;

namespace Core.Entities.Units
{
    // не может быть вылеченным
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
    }
}