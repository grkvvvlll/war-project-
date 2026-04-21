using System;
using Core.Interfaces;

namespace Core.Entities.Units
{
    public abstract class Unit : IUnit
    {
        public string Name { get; set; }
        public int Attack { get; }
        public int Defence { get; }
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Cost { get; }
        public virtual bool IsAlive => Health > 0;
        public ISpecialAbility? SpecialAbility { get; protected set; }

        public event Action<IUnit, int, int>? HealthChanged;
        public event Action<IUnit>? Died;

        protected virtual void OnHealthChanged(int oldHp, int newHp)
        {
            if (oldHp != newHp)
                HealthChanged?.Invoke(this, oldHp, newHp);
        }

        protected virtual void OnDied()
        {
            Died?.Invoke(this);
        }
        protected Unit(
            string name,
            int attack,
            int defence,
            int health,
            int cost)
            : this(name, attack, defence, health, health, cost)
        {
        }

        protected Unit(
            string name,
            int attack,
            int defence,
            int health,
            int maxHealth,
            int cost)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Unit name cannot be empty.");
            if (health < 0)
                throw new ArgumentException("Health cannot be negative.");
            if (maxHealth <= 0)
                throw new ArgumentException("MaxHealth must be greater than zero.");
            if (health > maxHealth)
                throw new ArgumentException("Health cannot be greater than MaxHealth.");

            Name = name;
            Attack = attack;
            Defence = defence;
            Health = health;
            MaxHealth = maxHealth;
            Cost = cost;
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentException("Damage cannot be negative.");

            int oldHp = Health;
            bool wasAlive = IsAlive;

            Health -= damage;
            if (Health < 0)
                Health = 0;

            OnHealthChanged(oldHp, Health);

            if (wasAlive && !IsAlive)
                OnDied();
        }

        public virtual void Heal(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Heal amount cannot be negative.");

            int oldHp = Health;

            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;

            OnHealthChanged(oldHp, Health);
        }

        public override string ToString()
        {
            return $"{Name} (HP: {Health}/{MaxHealth}, ATK: {Attack}, DEF: {Defence})";
        }
    }
}