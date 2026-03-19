using Core.Interfaces;
using Core.Entities.Units;

namespace Core.Factories.Units
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
