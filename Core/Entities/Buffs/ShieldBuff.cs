using Core.Interfaces;

namespace Core.Entities.Buffs
{
    public class ShieldBuff : IBuff
    {
        public string NameInstrumental => "Щитом";
        public string NameNominative => "Щит";
        public string BuffType => "Shield";
        public int AttackBonus => 0;
        public int DefenceBonus => 3;
    }

    public class ShieldDecorator : UnitDecorator
    {
        public ShieldDecorator(IUnit unit) : base(unit, new ShieldBuff()) { }
    }
}