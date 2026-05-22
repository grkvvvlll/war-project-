using Core.Interfaces;
using Core.Entities.Units;

namespace Core.Factories.Units
{
    public class HeavyUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Heavy";
        public override int UnitCost => UnitFactory.HeavyCost;

        public override IUnit CreateUnit(string name)
        {
            var unit = new HeavyUnit(
                name,
                UnitFactory.HeavyAttack,
                UnitFactory.HeavyDefence,
                UnitFactory.HeavyHP,
                UnitFactory.HeavyCost);

            return unit;
        }
    }
}
