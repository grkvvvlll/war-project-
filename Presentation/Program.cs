using Core.Formations;
using Core.Interfaces;
using Services.Battle;
using Services.Logging;
using Services.Random;
using Services.ArmyBuilding;
using Services.Formation;
using Services.UI;
using Core.Factories;
using Core.Factories.Armies;
using Core.Factories.Units;

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

            // Создание фабрик и ArmyBuilder
            var unitCreatorFactory = new UnitCreatorFactory(randomService);
            var unitCreators = unitCreatorFactory.Create();
            var autoFactory = new AutoArmyFactory(unitCreators);
            var manualFactory = new ManualArmyFactory(unitCreators);
            var armyBuilder = new ArmyBuilder(autoFactory, manualFactory);
            var formationSelector = new FormationSelector();
            var logCleaner = new LogCleaner((RecordingBattleLogger)logger);
            var armyPrinter = new ArmyPrinter();

            var menu = new ConsoleMenu(randomService, logger, damageCalculator, battleField, armyBuilder, formationSelector, logCleaner, armyPrinter);

            try
            {
                menu.Run();
            }
            finally
            {
            }
        }
    }
}