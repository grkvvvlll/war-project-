using Core.Factories;
using Core.Factories.Armies;
using Core.Formations;
using Core.Interfaces;
using Services.ArmyBuilding;
using Services.Battle;
using Services.Formation;
using Services.Logging;
using Services.Observers;
using Services.Random;
using Services.Storage;
using Services.UI;

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
            SpecialAbilityService specialAbilityService = new SpecialAbilityService(logger, randomService, formation);

            var saveService = new BattleSaveService();
            IBattleUI battleUI = new ConsoleBattleUI();

            IBattleField battleField = new BattleField(
                meleeService,
                specialAbilityService,
                randomService,
                logger,
                formation,
                saveService,
                battleUI);

            // Создание фабрик и ArmyBuilder
            var unitCreatorFactory = new UnitCreatorFactory(randomService);
            var unitCreators = unitCreatorFactory.Create();
            var autoFactory = new AutoArmyFactory(unitCreators, randomService);
            var manualFactory = new ManualArmyFactory(unitCreators);
            var armyBuilder = new ArmyBuilder(autoFactory, manualFactory);
            var manualArmySelector = new ManualArmySelector(manualFactory);
            var formationSelector = new FormationSelector();
            var logCleaner = new LogCleaner((RecordingBattleLogger)logger);
            var armyPrinter = new ArmyPrinter();

            var menu = new ConsoleMenu(
                randomService,
                logger,
                damageCalculator,
                battleField,
                armyBuilder,
                manualArmySelector,
                saveService,
                formationSelector,
                logCleaner,
                armyPrinter,
                new UnitRenumberer(),
                new CreationTypeSelector(),
                new ObserverAttacher(),
                new BudgetReader());

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