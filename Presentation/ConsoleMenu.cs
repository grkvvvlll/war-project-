using System;
using System.Collections.Generic;
using gaaameee.Core.Interfaces;
using gaaameee.Core.Entities;
using gaaameee.Core.Factories;
using Services.Battle;

namespace Presentation
{
    public class ConsoleMenu
    {
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleField _battleField;

        public ConsoleMenu(
            IRandomService random,
            IBattleLogger logger,
            IDamageCalculator damageCalculator,
            IBattleField battleField)
        {
            _random = random;
            _logger = logger;
            _damageCalculator = damageCalculator;
            _battleField = battleField;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Battle Game ===");
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Помощь");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        StartNewGame();
                        break;

                    case "2":
                        ShowHelp();
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Неверный выбор.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private void StartNewGame()
        {
            Console.Clear();

            Console.Write("Введите количество юнитов в армии 1: ");
            int count1 = ReadInt();

            Console.Write("Введите количество юнитов в армии 2: ");
            int count2 = ReadInt();

            var army1 = CreateRandomArmy("Армия 1", count1);
            var army2 = CreateRandomArmy("Армия 2", count2);

            Console.Clear();
            Console.WriteLine("Армии готовы:\n");

            BattleVisualizer.PrintArmyLine(army1, army2);

            Console.WriteLine("\nНажмите Enter для начала боя...");
            Console.ReadLine();

            var result = _battleField.StartBattle(army1, army2);

            Console.WriteLine($"\nПобедитель: {result.Winner}");
            Console.WriteLine($"Ходов: {result.Turns}");
            Console.ReadLine();
        }

        private IArmy CreateRandomArmy(string name, int count)
        {
            var units = new List<IUnit>();

            for (int i = 0; i < count; i++)
            {
                int type = _random.Next(0, 3);

                switch (type)
                {
                    case 0:
                        units.Add(UnitFactory.CreateHeavy($"Heavy {i + 1}"));
                        break;

                    case 1:
                        units.Add(UnitFactory.CreateLight($"Light {i + 1}"));
                        break;

                    case 2:
                        units.Add(UnitFactory.CreateArcher($"Archer {i + 1}"));
                        break;
                }
            }

            return new Army(name, units);
        }

        private void ShowHelp()
        {
            Console.Clear();
            Console.WriteLine("=== ПОМОЩЬ ===\n");

            var heavy = UnitFactory.CreateHeavy("Heavy");
            var light = UnitFactory.CreateLight("Light");
            var archer = UnitFactory.CreateArcher("Archer");

            PrintUnitInfo("🛡️ HeavyUnit", heavy);
            PrintUnitInfo("⚔️ LightUnit", light);
            PrintUnitInfo("🏹 Archer", archer);

            Console.WriteLine("Механика:");
            Console.WriteLine("A удар → B ответ → A лучники → B лучники → следующий ход.");
            Console.WriteLine("Урон = Attack - Defence (не меньше 0).");

            Console.WriteLine("\nНажмите Enter для возврата...");
            Console.ReadLine();
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

            Console.WriteLine();
        }

        private int ReadInt()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                    return value;

                Console.Write("Введите корректное число: ");
            }
        }
    }
}