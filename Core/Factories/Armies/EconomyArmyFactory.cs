using System;
using System.Collections.Generic;
using System.Linq;
using gaaameee.Core.Interfaces;
using gaaameee.Core.Entities;
using gaaameee.Core.Factories.Units;

namespace gaaameee.Core.Factories.Armies
{
    public class EconomyArmyFactory : IArmyFactory
    {
        public string FactoryName => "Экономная";

        private readonly Dictionary<string, UnitCreator> _unitCreators;

        // больше легких
        private const double HeavyWeight = 0.2;
        private const double LightWeight = 0.5;
        private const double ArcherWeight = 0.3;

        public EconomyArmyFactory(Dictionary<string, UnitCreator> unitCreators)
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
                ("Archer", UnitFactory.ArcherCost, ArcherWeight)
            };

            var budgets = new int[unitTypes.Count];
            for (int i = 0; i < unitTypes.Count; i++)
            {
                int baseBudget = (int)(budget * (unitTypes[i].weight / (HeavyWeight + LightWeight + ArcherWeight)));
                int variance = (int)(baseBudget * 0.15);
                int variation = random.Next(-variance, variance + 1);
                budgets[i] = Math.Max(0, baseBudget + variation);
            }

            var counters = new Dictionary<string, int>
            {
                { "Heavy", 0 },
                { "Light", 0 },
                { "Archer", 0 }
            };

            for (int i = 0; i < unitTypes.Count; i++)
            {
                int count = budgets[i] / unitTypes[i].cost;
                for (int j = 0; j < count; j++)
                {
                    counters[unitTypes[i].type]++;
                    units.Add(_unitCreators[unitTypes[i].type].CreateUnit($"{unitTypes[i].type} {counters[unitTypes[i].type]}"));
                }
            }

            int remaining = budget - units.Sum(u => u.Cost);
            while (remaining >= unitTypes.Min(t => t.cost))
            {
                var affordable = unitTypes.Where(t => t.cost <= remaining).ToList();
                var chosen = affordable[random.Next(affordable.Count)];
                counters[chosen.type]++;
                units.Add(_unitCreators[chosen.type].CreateUnit($"{chosen.type} {counters[chosen.type]}"));
                remaining -= chosen.cost;
            }

            if (units.Count == 0 && budget > 0)
            {
                var cheapest = unitTypes.OrderBy(t => t.cost).First();
                counters[cheapest.type]++;
                units.Add(_unitCreators[cheapest.type].CreateUnit($"{cheapest.type} {counters[cheapest.type]}"));
            }

            return new Army(name, units);
        }
    }
}