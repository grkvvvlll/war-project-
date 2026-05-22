using Core.Entities.Units;
using Core.Factories;
using Core.Interfaces;

namespace Services.UI
{
    public class ArmyPrinter
    {
        public void Print(IArmy army)
        {
            Console.WriteLine($"=== {army.Name} (Бюджет: {army.TotalCost} монет) ===");

            var heavyCount = army.Units.Count(u => GetBaseType(u) == typeof(HeavyUnit));
            var lightCount = army.Units.Count(u => GetBaseType(u) == typeof(LightUnit));
            var archerCount = army.Units.Count(u => GetBaseType(u) == typeof(Archer));
            var healerCount = army.Units.Count(u => GetBaseType(u) == typeof(Healer));
            var wizardCount = army.Units.Count(u => GetBaseType(u) == typeof(Wizard));
            var gulyayCount = army.Units.Count(u => GetBaseType(u) == typeof(GulyayGorodAdapter));

            Console.WriteLine($"🛡️ Тяжёлых: {heavyCount} × {UnitFactory.HeavyCost} = {heavyCount * UnitFactory.HeavyCost} монет");
            Console.WriteLine($"⚔️ Лёгких: {lightCount} × {UnitFactory.LightCost} = {lightCount * UnitFactory.LightCost} монет");
            Console.WriteLine($"🏹 Лучников: {archerCount} × {UnitFactory.ArcherCost} = {archerCount * UnitFactory.ArcherCost} монет");
            Console.WriteLine($"💚 Целителей: {healerCount} × {UnitFactory.HealerCost} = {healerCount * UnitFactory.HealerCost} монет");
            Console.WriteLine($"🔮 Магов: {wizardCount} × {UnitFactory.WizardCost} = {wizardCount * UnitFactory.WizardCost} монет");
            Console.WriteLine($"🏰 Гуляй-город: {gulyayCount} × {UnitFactory.GulyayGorodCost} = {gulyayCount * UnitFactory.GulyayGorodCost} монет");
            Console.WriteLine($"─────────────────────────────────────────");
            Console.WriteLine($"Всего юнитов: {army.Units.Count}");
            Console.WriteLine($"Итого потрачено: {army.TotalCost} монет");
            Console.WriteLine("\nСостав армии:");

            foreach (var unit in army.Units)
            {
                string icon = GetUnitTypeIcon(unit);
                Console.WriteLine($"  {icon} {unit.Name} (HP:{unit.Health} ATK:{unit.Attack} DEF:{unit.Defence})");
            }
        }

        private string GetUnitTypeIcon(IUnit unit)
        {
            IUnit current = unit;
            while (current is Core.Entities.Buffs.UnitDecorator decorator)
            {
                current = decorator.GetInnerUnit();
            }

            return current switch
            {
                HeavyUnit _ => "🛡️",
                LightUnit _ => "⚔️",
                Archer _ => "🏹",
                Healer _ => "💚",
                Wizard _ => "🔮",
                GulyayGorodAdapter _ => "🏰",
                _ => "❓"
            };
        }

        private Type GetBaseType(IUnit unit)
        {
            IUnit current = unit;
            while (current is Core.Entities.Buffs.UnitDecorator decorator)
            {
                current = decorator.GetInnerUnit();
            }
            return current.GetType();
        }
    }
}