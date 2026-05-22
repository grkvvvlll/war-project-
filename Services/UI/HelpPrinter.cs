using Core.Interfaces;
using Core.Entities.Units;
using Core.Factories.Units;
using Services.Random;

namespace Services.UI
{
    public class HelpPrinter
    {
        private readonly IRandomService _random;

        public HelpPrinter(IRandomService random)
        {
            _random = random;
        }

        public void Print()
        {
            var heavy = new HeavyUnitCreator().CreateUnit("Heavy");
            var light = new LightUnitCreator(_random).CreateUnit("Light");
            var archer = new ArcherUnitCreator().CreateUnit("Archer");
            var healer = new HealerUnitCreator().CreateUnit("Healer");
            var wizard = new WizardUnitCreator(new RandomService()).CreateUnit("Wizard");

            PrintUnitInfo("🛡️ HeavyUnit - сильный солдат:", heavy);
            PrintUnitInfo("⚔️ LightUnit - обычный солдат:", light);
            PrintUnitInfo("🏹 Archer - лучник:", archer);
            PrintUnitInfo("💚 Healer - целитель:", healer);
            PrintUnitInfo("🔮 Wizard - маг:", wizard);
            Console.WriteLine("🏰 Гуляй-город: огромная защита, не атакует, не лечится, не клонируется.\n");

            Console.WriteLine("Алгоритм игры:");
            Console.WriteLine("1. Случайным образом выбирается армия, атакующая первой.");
            Console.WriteLine("2. Ближайшие друг к другу солдаты вражеских армий наносят по одному удару.");
            Console.WriteLine("3. Юниты со SpecialAbility используют свои способности:");
            Console.WriteLine("   - 🏹 Лучники стреляют во врагов (если не на передней линии).");
            Console.WriteLine("   - 💚 Целители лечат союзников (кроме Heavy и себя).");
            Console.WriteLine("   - 🔮 Маги клонируют союзников (Light или Archer) с накоплением вероятности.");
            Console.WriteLine("4. Убитые солдаты исчезают.");
        }

        private void PrintUnitInfo(string title, IUnit unit)
        {
            Console.WriteLine(title);
            Console.WriteLine($"   HP: {unit.Health}");
            Console.WriteLine($"   ATK: {unit.Attack}");
            Console.WriteLine($"   DEF: {unit.Defence}");
            Console.WriteLine($"   COST: {unit.Cost}");
            if (unit is Archer archer)
                Console.WriteLine($"   RANGE: {archer.Range}");
            if (unit is Healer healer)
            {
                Console.WriteLine($"   HEAL_RANGE: {healer.HealRange}");
                Console.WriteLine($"   HEAL_POWER: {healer.HealPower}");
            }
            if (unit is Wizard wizard)
            {
                Console.WriteLine($"   SPELL_RANGE: {wizard.SpellRange}");
                Console.WriteLine($"   CLONE_CHANCE: {wizard.ClonePower}%");
            }
            Console.WriteLine();
        }
    }
}