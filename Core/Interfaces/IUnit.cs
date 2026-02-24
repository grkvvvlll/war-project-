namespace gaaameee.Core.Interfaces
{
    public interface IUnit
    {
        string Name { get; }

        int Attack { get; }
        int Defence { get; }
        int Health { get; }
        int Cost { get; }

        bool IsAlive { get; }

        ISpecialAbility? SpecialAbility { get; }

        void TakeDamage(int damage);
    }
}