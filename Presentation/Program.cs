using System;
using gaaameee.Core.Interfaces;
using Services.Battle;
using Services.Logging;
using Services.Random;
using gaaameee.Core.Entities;

namespace Presentation
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // 1️⃣ Создаём инфраструктуру
            IRandomService randomService = new RandomService();
            IBattleLogger logger = new ConsoleBattleLogger();
            IDamageCalculator damageCalculator = new DamageCalculator();
            IBattleField battleField = new BattleField(damageCalculator, logger, randomService);

            // 2️⃣ Создаём и запускаем консольное меню
            var menu = new ConsoleMenu(randomService, logger, damageCalculator, battleField);
            menu.Run();
        }

        // Метод для визуализации армий с человечками и символами типа
        public static void PrintArmyLine(IArmy army1, IArmy army2, int gap = 5)
        {
            int maxCount = Math.Max(army1.Units.Count, army2.Units.Count);
            string spacing = new string(' ', gap); // расстояние между армиями

            // Верхняя строка — символ типа юнита
            for (int i = 0; i < maxCount; i++)
            {
                // Армия 1
                if (i < army1.Units.Count && army1.Units[i].IsAlive)
                {
                    Console.ForegroundColor = GetColor(army1.Units[i]);
                    Console.Write(GetUnitTypeSymbol(army1.Units[i]));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" ");
                }
                Console.ResetColor();
                Console.Write(" ");
            }

            Console.Write(spacing); // <- gap между армиями

            // Армия 2
            for (int i = 0; i < maxCount; i++)
            {
                if (i < army2.Units.Count && army2.Units[i].IsAlive)
                {
                    Console.ForegroundColor = GetColor(army2.Units[i]);
                    Console.Write(GetUnitTypeSymbol(army2.Units[i]));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" ");
                }
                Console.ResetColor();
                Console.Write(" ");
            }
            Console.WriteLine();

            // Нижняя строка — человечки
            for (int i = 0; i < maxCount; i++)
            {
                // Армия 1
                if (i < army1.Units.Count && army1.Units[i].IsAlive)
                {
                    Console.ForegroundColor = GetColor(army1.Units[i]);
                    Console.Write("👤");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("·");
                }
                Console.ResetColor();
                Console.Write(" ");
            }

            Console.Write(spacing); // <- gap между армиями

            // Армия 2
            for (int i = 0; i < maxCount; i++)
            {
                if (i < army2.Units.Count && army2.Units[i].IsAlive)
                {
                    Console.ForegroundColor = GetColor(army2.Units[i]);
                    Console.Write("👤");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("·");
                }
                Console.ResetColor();
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        // Цвет по типу юнита
        private static ConsoleColor GetColor(IUnit unit)
        {
            if (unit is HeavyUnit) return ConsoleColor.Green;
            if (unit is LightUnit) return ConsoleColor.Yellow;
            if (unit is Archer) return ConsoleColor.Magenta;
            return ConsoleColor.White;
        }

        // Символ типа юнита (для линии над головами)
        private static string GetUnitTypeSymbol(IUnit unit)
        {
            if (unit is HeavyUnit) return "🛡️";
            if (unit is LightUnit) return "⚔️";
            if (unit is Archer) return "🏹";
            return " ";
        }
    }
}