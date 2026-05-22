using Core.Interfaces;

namespace Core.Entities.Buffs
{
    public class SpearBuff : IBuff
    {
        public string NameInstrumental => "Копьем";
        public string NameNominative => "Копьё";
        public string BuffType => "Spear";
        public int AttackBonus => 3;
        public int DefenceBonus => 0;
    }

    public class SpearDecorator : UnitDecorator
    {
        public SpearDecorator(IUnit unit) : base(unit, new SpearBuff()) { }
    }
}