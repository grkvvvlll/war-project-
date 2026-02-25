using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Entities
{
    public class Archer : Unit
    {
        // Дальность выстрела 
        public int Range { get; }

        // Конструктор лучника.
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

            // Назначаем специальную способность.
            SpecialAbility = new ArcherShotAbility(range);
        }
    }
}