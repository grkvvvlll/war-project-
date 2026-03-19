using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Factories
{
    public class ArcherUnitCreator : UnitCreator
    {
        public override string UnitTypeName => "Archer";
        public override int UnitCost => UnitFactory.ArcherCost;

        public override IUnit CreateUnit(string name)
        {
            return new Archer(
                name,
                UnitFactory.ArcherAttack,
                UnitFactory.ArcherDefence,
                UnitFactory.ArcherHP,
                UnitFactory.ArcherCost,
                UnitFactory.ArcherRange);
        }
    }
}
