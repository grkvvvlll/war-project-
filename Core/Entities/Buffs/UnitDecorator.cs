using Core.Interfaces;

namespace Core.Entities.Buffs
{
    public abstract class UnitDecorator : IUnit
    {
        protected readonly IUnit _unit;
        protected readonly IBuff _buff;
        private bool _isBroken = false;
        public IBuff? BrokenBuff { get; private set; }

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
                // 1. Находим самый внутренний (базовый) юнит, чтобы взять его "чистое" имя без скобок
                IUnit root = this;
                while (root is UnitDecorator decorator)
                {
                    root = decorator.GetInnerUnit();
                }
                string baseName = root.Name;

                // 2. Собираем все баффы в цепочке
                var allBuffs = GetAllBuffsList();

                if (!allBuffs.Any())
                    return baseName;

                // 3. Формируем красивый список: "с Бафф1, с Бафф2"
                string buffsString = string.Join(", ", allBuffs.Select(b => $"с {b.NameInstrumental}"));
                return $"{baseName} ({buffsString})";
            }
            set
            {
                // Пропускаем присваивание до базового юнита, чтобы не ломать цепочку
                IUnit root = this;
                while (root is UnitDecorator decorator)
                {
                    root = decorator.GetInnerUnit();
                }
                root.Name = value;
            }
        }

        public int Attack => _unit.Attack + _buff.AttackBonus;
        public int Defence => _unit.Defence + _buff.DefenceBonus;

        public int Health => _unit.Health;
        public int MaxHealth => _unit.MaxHealth;
        public int Cost => _unit.Cost;
        public bool IsAlive => _unit.IsAlive;

        public ISpecialAbility? SpecialAbility => _unit.SpecialAbility;
        public event Action<IUnit, int, int>? HealthChanged
        {
            add { _unit.HealthChanged += value; }
            remove { _unit.HealthChanged -= value; }
        }

        public event Action<IUnit>? Died
        {
            add { _unit.Died += value; }
            remove { _unit.Died -= value; }
        }

        public void TakeDamage(int damage)
        {
            // Передаём урон напрямую базовому юниту, минуя промежуточные декораторы
            IUnit inner = _unit;
            while (inner is UnitDecorator innerDec)
                inner = innerDec.GetInnerUnit();
            inner.TakeDamage(damage);

            if (damage == 0) return;

            int chanceToRemove = 20;
            if (damage > 5) chanceToRemove = 50;
            if (new Random().Next(0, 100) < chanceToRemove)
            {
                _isBroken = true;
                BrokenBuff = _buff;
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

        public override string ToString()
        {
            return $"{Name} (HP: {Health}/{MaxHealth}, ATK: {Attack}, DEF: {Defence})";
        }
    }
}