using Core.Entities;
using Core.Interfaces;
using Core.Factories.Units;

namespace Core.Factories.Armies
{
    public class AutoArmyFactory : IArmyFactory
    {
        public string FactoryName => "Автоматическая";

        private readonly Dictionary<string, UnitCreator> _unitCreators;
        private readonly System.Random _random;

        private const double HeavyWeight = 0.17;
        private const double LightWeight = 0.17;
        private const double ArcherWeight = 0.17;
        private const double HealerWeight = 0.17;
        private const double WizardWeight = 0.17;
        private const double GulyayGorodWeight = 0.15;

        public AutoArmyFactory(Dictionary<string, UnitCreator> unitCreators)
        {
            _unitCreators = unitCreators;
            _random = new System.Random();
        }

        public IArmy CreateArmy(string name, int budget)
        {
            var units = new List<IUnit>();

            var unitTypes = new List<(string type, int cost, double weight)>
            {
                ("Heavy", _unitCreators["Heavy"].UnitCost, HeavyWeight),
                ("Light", _unitCreators["Light"].UnitCost, LightWeight),
                ("Archer", _unitCreators["Archer"].UnitCost, ArcherWeight),
                ("Healer", _unitCreators["Healer"].UnitCost, HealerWeight),
                ("Wizard", _unitCreators["Wizard"].UnitCost, WizardWeight),
                ("GulyayGorod", _unitCreators["GulyayGorod"].UnitCost, GulyayGorodWeight)
            };

            var budgets = new int[unitTypes.Count];
            for (int i = 0; i < unitTypes.Count; i++)
            {
                int baseBudget = (int)(budget * (unitTypes[i].weight /
                    (HeavyWeight + LightWeight + ArcherWeight + HealerWeight + WizardWeight + GulyayGorodWeight)));
                int variance = (int)(baseBudget * 0.15);
                int variation = _random.Next(-variance, variance + 1);
                budgets[i] = Math.Max(0, baseBudget + variation);
            }

            var counters = new Dictionary<string, int>
            {
                { "Heavy", 0 }, { "Light", 0 }, { "Archer", 0 },
                { "Healer", 0 }, { "Wizard", 0 }, { "GulyayGorod", 0 }
            };

            for (int i = 0; i < unitTypes.Count; i++)
            {
                int count = budgets[i] / unitTypes[i].cost;
                for (int j = 0; j < count; j++)
                {
                    counters[unitTypes[i].type]++;
                    units.Add(_unitCreators[unitTypes[i].type].CreateUnit(
                        $"{unitTypes[i].type} {counters[unitTypes[i].type]}"));
                }
            }

            int remaining = budget - units.Sum(u => u.Cost);
            while (remaining >= unitTypes.Min(t => t.cost))
            {
                var affordable = unitTypes.Where(t => t.cost <= remaining).ToList();
                var chosen = affordable[_random.Next(affordable.Count)];
                counters[chosen.type]++;
                units.Add(_unitCreators[chosen.type].CreateUnit(
                    $"{chosen.type} {counters[chosen.type]}"));
                remaining -= chosen.cost;
            }

            if (units.Count == 0 && budget > 0)
            {
                var cheapest = unitTypes.OrderBy(t => t.cost).First();
                counters[cheapest.type]++;
                units.Add(_unitCreators[cheapest.type].CreateUnit(
                    $"{cheapest.type} {counters[cheapest.type]}"));
            }

            return new Army(name, units);
        }
    }
}