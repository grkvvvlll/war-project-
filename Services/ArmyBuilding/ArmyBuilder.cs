using Core.Factories.Armies;
using Core.Interfaces;

namespace Services.ArmyBuilding
{
    public class ArmyBuilder
    {
        private readonly AutoArmyFactory _autoFactory;
        private readonly ManualArmyFactory _manualFactory;

        public ArmyBuilder(AutoArmyFactory autoFactory, ManualArmyFactory manualFactory)
        {
            _autoFactory = autoFactory;
            _manualFactory = manualFactory;
        }

        public IArmy Build(string name, int budget, bool isAuto)
        {
            if (isAuto)
            {
                return _autoFactory.CreateArmy(name, budget);
            }
            else
            {
                var choices = GetManualUnitChoices(name, budget);
                return _manualFactory.CreateArmy(name, budget, choices);
            }
        }

        public List<string> GetManualUnitChoices(string armyName, int budget)
        {
            var choices = new List<string>();
            int spentBudget = 0;
            int minCost = _manualFactory.GetMinUnitCost();

            Console.WriteLine($"\n=== {armyName} (Бюджет: {budget} монет) ===");

            while (spentBudget + minCost <= budget)
            {
                int remaining = budget - spentBudget;
                Console.WriteLine($"\nОсталось: {remaining} монет");
                Console.WriteLine("Выберите юнита:");
                Console.WriteLine($"1. Heavy ({_manualFactory.GetUnitCost("Heavy")} монет)");
                Console.WriteLine($"2. Light ({_manualFactory.GetUnitCost("Light")} монет)");
                Console.WriteLine($"3. Archer ({_manualFactory.GetUnitCost("Archer")} монет)");
                Console.WriteLine($"4. Healer ({_manualFactory.GetUnitCost("Healer")} монет)");
                Console.WriteLine($"5. Wizard ({_manualFactory.GetUnitCost("Wizard")} монет)");
                Console.WriteLine($"6. GulyayGorod ({_manualFactory.GetUnitCost("GulyayGorod")} монет)");
                Console.WriteLine("0. Закончить формирование");
                Console.Write("Ваш выбор: ");
                var input = Console.ReadLine();

                if (input == "0")
                    break;

                var unitType = input switch
                {
                    "1" => "Heavy",
                    "2" => "Light",
                    "3" => "Archer",
                    "4" => "Healer",
                    "5" => "Wizard",
                    "6" => "GulyayGorod",
                    _ => null
                };

                if (unitType == null)
                {
                    Console.WriteLine("  Неверный выбор");
                    continue;
                }

                int cost = _manualFactory.GetUnitCost(unitType);
                if (cost > remaining)
                {
                    Console.WriteLine("  Недостаточно средств");
                    continue;
                }

                choices.Add(unitType);
                spentBudget += cost;
                Console.WriteLine($"  Добавлен {unitType} (-{cost} монет)");
            }

            if (choices.Count == 0)
            {
                Console.WriteLine("  Армия пуста, добавлен юнит по умолчанию");
                choices.Add("Light");
            }

            return choices;
        }
    }
}