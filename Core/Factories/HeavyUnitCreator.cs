using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Factories
{
    public class HeavyUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Heavy";
        public override int UnitCost => UnitFactory.HeavyCost;

        public override IUnit CreateUnit(string name)
        {
            return new HeavyUnit(
                name,
                UnitFactory.HeavyAttack,
                UnitFactory.HeavyDefence,
                UnitFactory.HeavyHP,
                UnitFactory.HeavyCost);
        }
    }
}
