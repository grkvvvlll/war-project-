using Core.Entities;
using Core.Factories.Armies;
using Core.Factories.Units;
using Core.Factories;
using Core.Interfaces;

namespace Core.Factories.Armies
{
    public class AggressiveArmyFactory : IArmyFactory
    {
        public string FactoryName => "Сильная";

        private readonly Dictionary<string, UnitCreator> _unitCreators;

        // Распределение бюджета (больше тяжёлых)
        private const double HeavyWeight = 0.40;
        private const double LightWeight = 0.20;
        private const double ArcherWeight = 0.15;
        private const double HealerWeight = 0.15;
        private const double WizardWeight = 0.10;

        public AggressiveArmyFactory(Dictionary<string, UnitCreator> unitCreators)
        {
            _unitCreators = unitCreators;
        }

        public IArmy CreateArmy(string name, int budget)
        {
            var units = new List<IUnit>();
            var random = new System.Random();

            var unitTypes = new List<(string type, int cost, double weight)>
            {
                ("Heavy", UnitFactory.HeavyCost, HeavyWeight),
                ("Light", UnitFactory.LightCost, LightWeight),
                ("Archer", UnitFactory.ArcherCost, ArcherWeight),
                ("Healer", UnitFactory.HealerCost, HealerWeight),
                ("Wizard", UnitFactory.WizardCost, WizardWeight)
            };

            var budgets = new int[unitTypes.Count];
            for (int i = 0; i < unitTypes.Count; i++)
            {
                int baseBudget = (int)(budget * (unitTypes[i].weight /
                    (HeavyWeight + LightWeight + ArcherWeight + HealerWeight + WizardWeight)));
                int variance = (int)(baseBudget * 0.15);
                int variation = random.Next(-variance, variance + 1);
                budgets[i] = Math.Max(0, baseBudget + variation);
            }

            var counters = new Dictionary<string, int>
            {
                { "Heavy", 0 },
                { "Light", 0 },
                { "Archer", 0 },
                { "Healer", 0 },
                { "Wizard", 0 }
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
                var chosen = affordable[random.Next(affordable.Count)];
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