using Core.Factories.Armies;
using Core.Interfaces;

namespace Services.ArmyBuilding
{
    public class ArmyBuilder
    {
        private readonly IArmyFactory _autoFactory;
        private readonly IArmyFactory _manualFactory;

        public ArmyBuilder(IArmyFactory autoFactory, IArmyFactory manualFactory)
        {
            _autoFactory = autoFactory;
            _manualFactory = manualFactory;
        }

        /// <summary>Создать армию автоматически.</summary>
        public IArmy Build(string name, int budget)
        {
            _autoFactory.PrepareCreation();
            return _autoFactory.CreateArmy(name, budget);
        }

        /// <summary>Создать армию вручную по заранее собранному списку типов юнитов.</summary>
        public IArmy Build(string name, int budget, IEnumerable<string> unitChoices)
        {
            _manualFactory.PrepareCreation(unitChoices);
            return _manualFactory.CreateArmy(name, budget);
        }
    }
}