using Core.Entities;
using Core.Interfaces;
using Core.Factories.Units;

namespace Core.Factories.Armies
{
    public class ManualArmyFactory
    {
        private readonly Dictionary<string, UnitCreator> _unitCreators;

        public ManualArmyFactory(Dictionary<string, UnitCreator> unitCreators)
        {
            _unitCreators = unitCreators;
        }

        public int GetUnitCost(string unitType)
        {
            return _unitCreators.TryGetValue(unitType, out var creator)
                ? creator.UnitCost
                : 0;
        }

        public int GetMinUnitCost()
        {
            return _unitCreators.Values.Min(c => c.UnitCost);
        }

        public IArmy CreateArmy(string name, int budget, List<string> unitChoices)
        {
            var units = new List<IUnit>();
            int spentBudget = 0;
            var counters = new Dictionary<string, int>
            {
                { "Heavy", 0 }, { "Light", 0 }, { "Archer", 0 },
                { "Healer", 0 }, { "Wizard", 0 }, { "GulyayGorod", 0 }
            };

            foreach (var choice in unitChoices)
            {
                if (!_unitCreators.TryGetValue(choice, out var creator))
                    continue;

                if (spentBudget + creator.UnitCost > budget)
                    break;

                counters[choice]++;
                units.Add(creator.CreateUnit($"{choice} {counters[choice]}"));
                spentBudget += creator.UnitCost;
            }

            if (units.Count == 0 && budget > 0)
            {
                var cheapest = _unitCreators.Values.OrderBy(c => c.UnitCost).First();
                units.Add(cheapest.CreateUnit($"{cheapest.UnitTypeName} 1"));
            }

            return new Army(name, units);
        }
    }
}