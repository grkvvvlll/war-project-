using System;
using gaaameee.Core.Interfaces;
using gaaameee.Core.Entities;

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
            if (unit is Archer)
                return "🏹";

            if (unit is HeavyUnit)
                return "🛡️";

            if (unit is LightUnit)
                return "⚔️";

            return "?";
        }
    }
}