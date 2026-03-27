using Core.Entities.Units;
using Core.Interfaces;
using Core.Entities.Units.Proxies;

namespace Core.Factories.Units
{
    public class HealerUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Healer";
        public override int UnitCost => UnitFactory.HealerCost;

        public override IUnit CreateUnit(string name)
        {
            var unit = new Healer(
                name,
                UnitFactory.HealerAttack,
                UnitFactory.HealerDefence,
                UnitFactory.HealerHP,
                UnitFactory.HealerCost,
                UnitFactory.HealerRange,
                UnitFactory.HealerPower);

            return new HealerProxy(unit);
        }
    }
}
