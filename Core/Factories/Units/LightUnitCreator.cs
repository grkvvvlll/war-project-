using Core.Interfaces;
using Core.Entities.Units;
using Core.Entities.Units.Proxies;

namespace Core.Factories.Units
{
    public class LightUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Light";
        public override int UnitCost => UnitFactory.LightCost;

        public override IUnit CreateUnit(string name)
        {
            var unit = new LightUnit(
                name,
                UnitFactory.LightAttack,
                UnitFactory.LightDefence,
                UnitFactory.LightHP,
                UnitFactory.LightCost);

            return new LightUnitProxy(unit);
        }
    }
}
