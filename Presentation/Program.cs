using Services.Battle;
using Services.Random;
using Services.Logging;
using Core.Interfaces;

namespace Presentation
{
    public class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IRandomService randomService = new RandomService();
            IBattleLogger logger = new RecordingBattleLogger(new ConsoleBattleLogger());
            IDamageCalculator damageCalculator = new DamageCalculator();
            IMeleeService meleeService = new MeleeService(damageCalculator, logger);

            SpecialAbilityService specialAbilityService = new SpecialAbilityService(logger);

            IBattleField battleField = new BattleField(
                meleeService,
                specialAbilityService,
                randomService,
                logger);

            var menu = new ConsoleMenu(randomService, logger, damageCalculator, battleField);
            menu.Run();
        }
    }
}