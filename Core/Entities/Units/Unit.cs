using System;
using Core.Interfaces;
using Core.Entities.Abilities;

namespace Core.Entities.Units
{
    public abstract class Unit : IUnit
    {
        public string Name { get; set; }
        public int Attack { get; }
        public int Defence { get; }
        public int Health { get; private set; }
        public int MaxHealth { get; }
        public int Cost { get; }
        public bool IsAlive => Health > 0;
        public ISpecialAbility? SpecialAbility { get; protected set; }

        protected Unit(
            string name,
            int attack,
            int defence,
            int health,
            int cost)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Unit name cannot be empty.");
            if (health <= 0)
                throw new ArgumentException("Health must be greater than zero.");

            Name = name;
            Attack = attack;
            Defence = defence;
            Health = health;
            MaxHealth = health;
            Cost = cost;
        }

        public void TakeDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentException("Damage cannot be negative.");

            Health -= damage;
            if (Health < 0)
                Health = 0;
        }

        public void Heal(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Heal amount cannot be negative.");

            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public override string ToString()
        {
            return $"{Name} (HP: {Health}/{MaxHealth}, ATK: {Attack}, DEF: {Defence})";
        }
    }
}