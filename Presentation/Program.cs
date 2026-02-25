using System;
using gaaameee.Core.Interfaces;
using Services.Battle;
using Services.Random;
using Services.Logging;

namespace Presentation
{
    public class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IRandomService randomService = new RandomService();
            IBattleLogger logger = new ConsoleBattleLogger();
            IDamageCalculator damageCalculator = new DamageCalculator();
            IBattleField battleField = new BattleField(damageCalculator, logger, randomService);

            var menu = new ConsoleMenu(randomService, logger, damageCalculator, battleField);
            menu.Run();
        }
    }
}