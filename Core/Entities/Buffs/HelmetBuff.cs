using Core.Interfaces;

namespace Core.Entities.Buffs
{
    public class HelmetBuff : IBuff
    {
        public string NameInstrumental => "Шлемом";
        public string NameNominative => "Шлем";
        public string BuffType => "Helmet";
        public int AttackBonus => 0;
        public int DefenceBonus => 2;
    }

    public class HelmetDecorator : UnitDecorator
    {
        public HelmetDecorator(IUnit unit) : base(unit, new HelmetBuff()) { }
    }
}