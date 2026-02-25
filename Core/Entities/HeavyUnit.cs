using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Entities
{
    // Имеет повышенную защиту и здоровье.
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
            // Спецспособности нет
            SpecialAbility = null;
        }
    }
}