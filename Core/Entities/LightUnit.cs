using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Entities
{
    public class LightUnit : Unit
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
    }
}