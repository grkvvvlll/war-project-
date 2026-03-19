
using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Factories
{
    public class LightUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Light";
        public override int UnitCost => UnitFactory.LightCost;

        public override IUnit CreateUnit(string name)
        {
            return new LightUnit(
                name,
                UnitFactory.LightAttack,
                UnitFactory.LightDefence,
                UnitFactory.LightHP,
                UnitFactory.LightCost);
        }
    }
}
