using System;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Entities
{
    public abstract class Unit : IUnit
    {
        // Имя юнита 
        public string Name { get; }

        // Сила атаки
        public int Attack { get; }

        // Защита юнита
        public int Defence { get; }

        // Текущее здоровье
        public int Health { get; private set; }

        // Стоимость юнита
        public int Cost { get; }

        // Жив ли юнит
        public bool IsAlive => Health > 0;

        // Специальная способность 
        public ISpecialAbility? SpecialAbility { get; protected set; }

        // Конструктор базового юнита.
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
            Cost = cost;
        }

        // Получение урона
        public void TakeDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentException("Damage cannot be negative.");

            Health -= damage;

            if (Health < 0)
                Health = 0;
        }

        public override string ToString()
        {
            return $"{Name} (HP: {Health}, ATK: {Attack}, DEF: {Defence})";
        }
    }
}