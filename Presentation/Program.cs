using Core.Formations;
using Core.Interfaces;
using Services.Battle;
using Services.Logging;
using Services.Random;

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

            // Построение по умолчанию — бой на мосту
            IBattleFormation formation = new BridgeFormation();

            IMeleeService meleeService = new MeleeService(damageCalculator, logger, formation);
            SpecialAbilityService specialAbilityService = new SpecialAbilityService(logger, formation);

            IBattleField battleField = new BattleField(
                meleeService,
                specialAbilityService,
                randomService,
                logger,
                formation);

            var menu = new ConsoleMenu(randomService, logger, damageCalculator, battleField);

            try
            {
                menu.Run();
            }
            finally
            {
                ConsoleMenu.RestoreConsoleScreen();
            }
        }
    }
}
