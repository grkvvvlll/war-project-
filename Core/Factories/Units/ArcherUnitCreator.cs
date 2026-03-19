using Core.Interfaces;
using Core.Entities.Units;

namespace Core.Factories.Units
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
