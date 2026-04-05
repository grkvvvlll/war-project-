using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;

namespace Core.Entities.Buffs
{
    public abstract class UnitDecorator : IUnit
    {
        protected readonly IUnit _unit;
        protected readonly IBuff _buff;
        private bool _isBroken = false;

        public UnitDecorator(IUnit unit, IBuff buff)
        {
            _unit = unit;
            _buff = buff;
        }

        // НОВОЕ: Формируем красивое имя со списком всех баффов
        public string Name
        {
            get
            {
                // Получаем все баффы в цепочке
                var allBuffs = GetAllBuffsList();

                if (!allBuffs.Any())
                    return _unit.Name;

                // Формируем строку: "с Щитом, с Копьем"
                string buffsString = string.Join(", ", allBuffs.Select(b => $"с {b.NameInstrumental}"));

                // Если базовый юнит уже имеет имя (например, Heavy 1), добавляем скобки
                return $"{_unit.Name} ({buffsString})";
            }
            set => _unit.Name = value;
        }

        public int Attack => _unit.Attack + _buff.AttackBonus;
        public int Defence => _unit.Defence + _buff.DefenceBonus;

        public int Health => _unit.Health;
        public int MaxHealth => _unit.MaxHealth;
        public int Cost => _unit.Cost;
        public bool IsAlive => _unit.IsAlive;

        public ISpecialAbility? SpecialAbility => _unit.SpecialAbility;

        public void TakeDamage(int damage)
        {
            _unit.TakeDamage(damage);

            int chanceToRemove = 20;
            if (damage > 5) chanceToRemove = 50;

            if (new Random().Next(0, 100) < chanceToRemove)
            {
                _isBroken = true;
            }
        }

        public void Heal(int amount)
        {
            _unit.Heal(amount);
        }

        public IUnit GetInnerUnit()
        {
            return _unit;
        }

        public IBuff GetCurrentBuff()
        {
            return _buff;
        }

        public bool IsBroken()
        {
            return _isBroken;
        }

        // Возвращает список всех баффов в цепочке (от внешнего к внутреннему)
        public List<IBuff> GetAllBuffsList()
        {
            var buffs = new List<IBuff>();
            if (_buff != null) buffs.Add(_buff);

            if (_unit is UnitDecorator decorator)
            {
                buffs.AddRange(decorator.GetAllBuffsList());
            }
            return buffs;
        }

        // Возвращает HashSet типов баффов (для проверки дубликатов)
        public HashSet<string> GetAllBuffTypes()
        {
            var types = new HashSet<string>();
            if (_buff != null) types.Add(_buff.BuffType);

            if (_unit is UnitDecorator decorator)
            {
                foreach (var t in decorator.GetAllBuffTypes())
                {
                    types.Add(t);
                }
            }
            return types;
        }
    }
}