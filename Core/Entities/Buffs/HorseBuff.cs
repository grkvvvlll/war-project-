using Core.Interfaces;

namespace Core.Entities.Buffs
{
    public class HorseBuff : IBuff
    {
        public string NameInstrumental => "Конем";
        public string NameNominative => "Конь";
        public string BuffType => "Horse";
        public int AttackBonus => 2;
        public int DefenceBonus => 2;
    }

    public class HorseDecorator : UnitDecorator
    {
        public HorseDecorator(IUnit unit) : base(unit, new HorseBuff()) { }
    }
}