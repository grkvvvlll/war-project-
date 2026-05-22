using Core.Entities.Units;
using Core.Interfaces;

namespace Core.Factories.Units
{
    public class WizardUnitCreator : UnitCreator
    {
        private readonly IRandomService _random;

        public override string UnitTypeName => "Wizard";
        public override int UnitCost => UnitFactory.WizardCost;

        public WizardUnitCreator(IRandomService random)
        {
            _random = random;
        }

        public override IUnit CreateUnit(string name)
        {
            var unit = new Wizard(
                name,
                UnitFactory.WizardAttack,
                UnitFactory.WizardDefence,
                UnitFactory.WizardHP,
                UnitFactory.WizardCost,
                UnitFactory.WizardRange,
                UnitFactory.WizardCloneChance,
                _random);

            return unit;
        }
    }
}
