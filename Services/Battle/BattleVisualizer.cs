using System;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Interfaces;

namespace Services.Battle
{
    /// <summary>
    /// Вспомогательный класс для визуализации армий и юнитов.
    /// Используется BattleField и Presentation.
    /// </summary>
    public static class BattleVisualizer
    {
        public static void PrintArmyLine(IArmy army1, IArmy army2)
        {
            int count1 = army1.Units.Count;
            int count2 = army2.Units.Count;

            // === ИКОНКИ ===
            for (int i = 0; i < count1; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(GetIcon(army1.Units[i]) + " ");
                Console.ResetColor();
            }

            Console.Write("     "); // расстояние между армиями

            for (int i = 0; i < count2; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(GetIcon(army2.Units[i]) + " ");
                Console.ResetColor();
            }

            Console.WriteLine();

            // === ЧЕЛОВЕЧКИ ===
            for (int i = 0; i < count1; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(army1.Units[i].IsAlive ? "👤 " : "· ");
                Console.ResetColor();
            }

            Console.Write("     ");

            for (int i = 0; i < count2; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(army2.Units[i].IsAlive ? "👤 " : "· ");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        private static string GetIcon(IUnit unit)
        {
            // Если это декоратор, смотрим внутрь
            IUnit actualUnit = unit;
            while (actualUnit is UnitDecorator decorator)
            {
                actualUnit = decorator.GetInnerUnit();
            }

            if (actualUnit is Archer) return "🏹";
            if (actualUnit is HeavyUnit) return "🛡️";
            if (actualUnit is LightUnit) return "⚔️";
            if (actualUnit is Wizard) return "🔮";
            if (actualUnit is Healer) return "💚";
            if (actualUnit is GulyayGorodAdapter) return "🏰";
            return "?";
        }
    }
}