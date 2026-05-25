using Core.Factories.Armies;

namespace Presentation
{
    // Отвечает за сбор выбора юнитов от пользователя через консоль.
    public class ManualArmySelector
    {
        private readonly ManualArmyFactory _factory;

        public ManualArmySelector(ManualArmyFactory factory)
        {
            _factory = factory;
        }

        public List<string> GetUnitChoices(string armyName, int budget)
        {
            var choices = new List<string>();
            int spentBudget = 0;
            int minCost = _factory.GetMinUnitCost();

            Console.WriteLine($"\n=== {armyName} (Бюджет: {budget} монет) ===");

            while (spentBudget + minCost <= budget)
            {
                int remaining = budget - spentBudget;
                Console.WriteLine($"\nОсталось: {remaining} монет");
                Console.WriteLine("Выберите юнита:");
                Console.WriteLine($"1. Heavy ({_factory.GetUnitCost("Heavy")} монет)");
                Console.WriteLine($"2. Light ({_factory.GetUnitCost("Light")} монет)");
                Console.WriteLine($"3. Archer ({_factory.GetUnitCost("Archer")} монет)");
                Console.WriteLine($"4. Healer ({_factory.GetUnitCost("Healer")} монет)");
                Console.WriteLine($"5. Wizard ({_factory.GetUnitCost("Wizard")} монет)");
                Console.WriteLine($"6. GulyayGorod ({_factory.GetUnitCost("GulyayGorod")} монет)");
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

                int cost = _factory.GetUnitCost(unitType);
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