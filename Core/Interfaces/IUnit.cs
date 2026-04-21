namespace Core.Interfaces
{
    public interface IUnit
    {
        string Name { get; set; }
        int Attack { get; }
        int Defence { get; }
        int Health { get; }
        int MaxHealth { get; }  // макс хп, чтобы оно не росло бесконечно
        int Cost { get; }
        bool IsAlive { get; }
        ISpecialAbility? SpecialAbility { get; }
        event Action<IUnit, int, int>? HealthChanged;
        event Action<IUnit>? Died;
        void TakeDamage(int damage);
        void Heal(int amount);  // лечить
    }
}