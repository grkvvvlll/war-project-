using System;
using System.Collections.Generic;
using gaaameee.Core.Interfaces;
using Services.Battle;
using Services.Logging;
using Services.Random;
using gaaameee.Core.Entities;

namespace Presentation
{
    public class ConsoleMenu
    {
        private readonly IRandomService _randomService;
        private readonly IBattleLogger _logger;
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleField _battleField;

        public ConsoleMenu(
            IRandomService randomService,
            IBattleLogger logger,
            IDamageCalculator damageCalculator,
            IBattleField battleField)
        {
            _randomService = randomService;
            _logger = logger;
            _damageCalculator = damageCalculator;
            _battleField = battleField;
        }

        public void Run()
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== Battle Game Menu ===");
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Загрузить игру");
                Console.WriteLine("3. Помощь");
                Console.WriteLine("4. Выход");
                Console.Write("Выберите пункт: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        StartNewGame();
                        break;
                    case "2":
                        LoadGame();
                        break;
                    case "3":
                        ShowHelp();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void StartNewGame()
        {
            Console.Clear();
            Console.WriteLine("=== Новая игра ===");

            int army1Count = ReadInt("Введите количество юнитов для Army 1: ");
            int army2Count = ReadInt("Введите количество юнитов для Army 2: ");

            var army1 = new Army("Army 1", GenerateRandomArmy(army1Count, _randomService));
            var army2 = new Army("Army 2", GenerateRandomArmy(army2Count, _randomService));

            Console.WriteLine("\n=== Начальные ряды армий ===");
            Program.PrintArmyLine(army1, army2); // визуализация с человечками и символами

            _battleField.StartBattle(army1, army2);

            Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в меню...");
            Console.ReadKey();
        }

        private void LoadGame()
        {
            Console.Clear();
            Console.WriteLine("=== Загрузить игру ===");
            Console.WriteLine("Функция пока не реализована.");
            Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в меню...");
            Console.ReadKey();
        }

        private void ShowHelp()
        {
            Console.Clear();
            Console.WriteLine("=== Помощь ===");
            Console.WriteLine("1. Новая игра — начать новый бой между двумя армиями.");
            Console.WriteLine("2. Загрузить игру — восстановить сохранённую игру (пока не реализовано).");
            Console.WriteLine("3. Выход — закрыть программу.");
            Console.WriteLine("Во время боя юниты отображаются как 👤, над ними символ типа: 🛡️ щит (Heavy), ⚔️ меч (Light), 🏹 лук (Archer).");
            Console.WriteLine("Мёртвые юниты становятся серыми или заменяются на '·'.");
            Console.WriteLine("\nНажмите любую клавишу, чтобы вернуться в меню...");
            Console.ReadKey();
        }

        private int ReadInt(string prompt)
        {
            int value;
            do
            {
                Console.Write(prompt);
            } while (!int.TryParse(Console.ReadLine(), out value) || value <= 0);
            return value;
        }

        private List<IUnit> GenerateRandomArmy(int count, IRandomService random)
        {
            var units = new List<IUnit>();

            for (int i = 0; i < count; i++)
            {
                int r = random.Next(0, 3); // 0,1,2
                switch (r)
                {
                    case 0: units.Add(new HeavyUnit("Heavy Soldier", 8, 6, 25, 50)); break;
                    case 1: units.Add(new LightUnit("Light Soldier", 10, 3, 15, 30)); break;
                    case 2: units.Add(new Archer("Archer", 7, 2, 12, 40, 5)); break;
                }
            }

            // Перемешиваем армию
            return units.OrderBy(u => random.Next(0, 1000)).ToList();
        }
    }
}