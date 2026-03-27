using Core.Interfaces;
using Core.Entities.Units;
using Core.Entities.Units.Proxies;

namespace Core.Factories.Units
{
    public class ArcherUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Archer";
        public override int UnitCost => UnitFactory.ArcherCost;

        public override IUnit CreateUnit(string name)
        {
            var unit = new Archer(
                name,
                UnitFactory.ArcherAttack,
                UnitFactory.ArcherDefence,
                UnitFactory.ArcherHP,
                UnitFactory.ArcherCost,
                UnitFactory.ArcherRange);

            return new ArcherProxy(unit);
        }
    }
}
