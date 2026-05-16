using Core.Factories.Units;
using Core.Interfaces;

namespace Core.Factories
{
    public class UnitCreatorFactory
    {
        private readonly IRandomService _random;

        public UnitCreatorFactory(IRandomService random)
        {
            _random = random;
        }

        public Dictionary<string, UnitCreator> Create()
        {
            return new Dictionary<string, UnitCreator>
            {
                { "Heavy", new HeavyUnitCreator() },
                { "Light", new LightUnitCreator(_random) },
                { "Archer", new ArcherUnitCreator() },
                { "Healer", new HealerUnitCreator() },
                { "Wizard", new WizardUnitCreator(_random) },
                { "GulyayGorod", new GulyayGorodCreator() }
            };
        }
    }
}