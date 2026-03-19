using Services.Battle;
using Services.Logging;
using Services.Random;
using Services.Storage;
using Core.Entities.Units;
using Core.Entities;
using Core.Factories.Armies;
using Core.Factories.Units;
using Core.Factories;
using Core.Interfaces;

namespace Presentation
{
    public class ConsoleMenu
    {
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;
        private readonly IDamageCalculator _damageCalculator;
        private readonly IBattleField _battleField;

        // === FACTORY METHOD: Словарь создателей юнитов ===
        private readonly Dictionary<string, UnitCreator> _unitCreators;

        // === ABSTRACT FACTORY: Словарь фабрик армий ===
        private readonly Dictionary<string, IArmyFactory> _armyFactories;

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

            // === ИНИЦИАЛИЗАЦИЯ FACTORY METHOD ===
            _unitCreators = new Dictionary<string, UnitCreator>
            {
                { "Heavy", new HeavyUnitCreator() },
                { "Light", new LightUnitCreator() },
                { "Archer", new ArcherUnitCreator() },
                { "Healer", new HealerUnitCreator() },
                { "Wizard", new WizardUnitCreator(random) }
            };

            // === ИНИЦИАЛИЗАЦИЯ ABSTRACT FACTORY ===
            _armyFactories = new Dictionary<string, IArmyFactory>
            {
                { "Standard", new StandardArmyFactory(_unitCreators) },
                { "Aggressive", new AggressiveArmyFactory(_unitCreators) },
                { "Economy", new EconomyArmyFactory(_unitCreators) }
            };
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Army Game ===");
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Помощь");
                Console.WriteLine("3. Загрузить игру");
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
                    case "3":
                        LoadGame();
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
            if (_logger is RecordingBattleLogger rec)
                rec.Clear();

            // === АРМИЯ 1: ВЫБОР ФАБРИКИ ===
            Console.WriteLine("=== ФОРМИРОВАНИЕ АРМИИ 1 ===");
            string factoryType1 = SelectArmyFactory();

            Console.Write("Введите стоимость для армии 1: ");
            int budget1 = ReadInt();

            // === АРМИЯ 2: ВЫБОР ФАБРИКИ ===
            Console.WriteLine("\n=== ФОРМИРОВАНИЕ АРМИИ 2 ===");
            string factoryType2 = SelectArmyFactory();

            Console.Write("Введите стоимость для армии 2: ");
            int budget2 = ReadInt();

            // === СОЗДАНИЕ АРМИЙ ЧЕРЕЗ ABSTRACT FACTORY ===
            var armyFactory1 = _armyFactories[factoryType1];
            var armyFactory2 = _armyFactories[factoryType2];

            var army1 = armyFactory1.CreateArmy("Армия 1", budget1);
            var army2 = armyFactory2.CreateArmy("Армия 2", budget2);

            Console.Clear();
            Console.WriteLine($"Армии сформированы:");
            Console.WriteLine($"  Армия 1: {armyFactory1.FactoryName}");
            Console.WriteLine($"  Армия 2: {armyFactory2.FactoryName}");
            Console.WriteLine();
            PrintArmyComposition(army1);
            Console.WriteLine();
            PrintArmyComposition(army2);
            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для начала боя...");
            Console.ReadLine();

            var result = _battleField.StartBattle(army1, army2);

            Console.WriteLine($"\nПобедитель: {result.Winner}");
            Console.WriteLine($"Ходов: {result.Turns}");
            AskToSaveBattle(result);
            Console.ReadLine();
        }

        private string SelectArmyFactory()
        {
            Console.WriteLine("Выберите тип армии:");
            Console.WriteLine("1. Стандартная (сбалансированная)");
            Console.WriteLine("2. Агрессивная (больше тяжёлых)");
            Console.WriteLine("3. Экономная (больше лёгких)");
            Console.Write("Ваш выбор (1-3): ");

            var choice = Console.ReadLine();
            return choice switch
            {
                "2" => "Aggressive",
                "3" => "Economy",
                _ => "Standard"
            };
        }

        private void PrintArmyComposition(IArmy army)
        {
            Console.WriteLine($"=== {army.Name} (Бюджет: {army.TotalCost} монет) ===");
            var heavyCount = army.Units.Count(u => u is HeavyUnit);
            var lightCount = army.Units.Count(u => u is LightUnit);
            var archerCount = army.Units.Count(u => u is Archer);
            var healerCount = army.Units.Count(u => u is Healer);
            var wizardCount = army.Units.Count(u => u is Wizard);

            Console.WriteLine($"🛡️ Тяжёлых: {heavyCount} × {UnitFactory.HeavyCost} = {heavyCount * UnitFactory.HeavyCost} монет");
            Console.WriteLine($"⚔️ Лёгких: {lightCount} × {UnitFactory.LightCost} = {lightCount * UnitFactory.LightCost} монет");
            Console.WriteLine($"🏹 Лучников: {archerCount} × {UnitFactory.ArcherCost} = {archerCount * UnitFactory.ArcherCost} монет");
            Console.WriteLine($"💚 Целителей: {healerCount} × {UnitFactory.HealerCost} = {healerCount * UnitFactory.HealerCost} монет");
            Console.WriteLine($"🔮 Магов: {wizardCount} × {UnitFactory.WizardCost} = {wizardCount * UnitFactory.WizardCost} монет");
            Console.WriteLine($"─────────────────────────────────────────");
            Console.WriteLine($"Всего юнитов: {army.Units.Count}");
            Console.WriteLine($"Итого потрачено: {army.TotalCost} монет");

            Console.WriteLine("\nСостав армии:");
            foreach (var unit in army.Units)
            {
                string icon = unit switch
                {
                    HeavyUnit _ => "🛡️",
                    LightUnit _ => "⚔️",
                    Archer _ => "🏹",
                    Healer _ => "💚",
                    Wizard _ => "🔮",
                    _ => "❓"
                };
                Console.WriteLine($"  {icon} {unit.Name} (HP:{unit.Health} ATK:{unit.Attack} DEF:{unit.Defence})");
            }
        }

        private void ShowHelp()
        {
            Console.Clear();

            // === ИСПОЛЬЗУЕМ FACTORY METHOD ДЛЯ ПРИМЕРА ===
            var heavy = new HeavyUnitCreator().CreateUnit("Heavy");
            var light = new LightUnitCreator().CreateUnit("Light");
            var archer = new ArcherUnitCreator().CreateUnit("Archer");
            var healer = new HealerUnitCreator().CreateUnit("Healer");
            var wizard = new WizardUnitCreator(new RandomService()).CreateUnit("Wizard");

            PrintUnitInfo("🛡️ HeavyUnit - сильный солдат:", heavy);
            PrintUnitInfo("⚔️ LightUnit - обычный солдат:", light);
            PrintUnitInfo("🏹 Archer - лучник:", archer);
            PrintUnitInfo("💚 Healer - целитель:", healer);
            PrintUnitInfo("🔮 Wizard - маг:", wizard);

            Console.WriteLine("Алгоритм игры:");
            Console.WriteLine("1. Случайным образом выбирается армия, атакующая первой.");
            Console.WriteLine("2. Ближайшие друг к другу солдаты вражеских армий наносят по одному удару.");
            Console.WriteLine("3. Юниты со SpecialAbility используют свои способности:");
            Console.WriteLine("   - 🏹 Лучники стреляют во врагов (если не на передней линии).");
            Console.WriteLine("   - 💚 Целители лечат союзников (кроме Heavy и себя).");
            Console.WriteLine("   - 🔮 Маги клонируют союзников (Light или Archer) с накоплением вероятности.");
            Console.WriteLine("4. Убитые солдаты исчезают.");
            Console.WriteLine("\nНажмите Enter для возврата в меню");
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
            if (unit is Healer healer)
            {
                Console.WriteLine($"   HEAL_RANGE: {healer.HealRange}");
                Console.WriteLine($"   HEAL_POWER: {healer.HealPower}");
            }
            if (unit is Wizard wizard)
            {
                Console.WriteLine($"   SPELL_RANGE: {wizard.SpellRange}");
                Console.WriteLine($"   CLONE_CHANCE: {wizard.ClonePower}%");
            }
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

        private void AskToSaveBattle(BattleResult result)
        {
            if (_logger is not RecordingBattleLogger rec)
            {
                Console.WriteLine("\n(Сохранение недоступно: логгер не RecordingBattleLogger)");
                return;
            }

            Console.Write("\nСохранить бой в файл? (y/n): ");
            var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (ans != "y" && ans != "yes" && ans != "д" && ans != "да")
                return;

            // === SINGLETON ===
            var saveService = BattleSaveService.Instance;

            var save = new BattleSave
            {
                Winner = result.Winner,
                Turns = result.Turns,
                LogLines = rec.Lines.ToList()
            };
            var fileName = saveService.Save(save);
            Console.WriteLine($"Сохранено: saves/{fileName}");
        }

        private void LoadGame()
        {
            Console.Clear();

            // === SINGLETON ===
            var saveService = BattleSaveService.Instance;

            var saves = saveService.ListSaves();

            if (saves.Count == 0)
            {
                Console.WriteLine("Сохранений нет. Нажмите Enter.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("=== Сохранённые бои ===");
            for (int i = 0; i < saves.Count; i++)
            {
                var s = saves[i];
                Console.WriteLine($"{i + 1}. {s.FileName} | {s.SavedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Победитель: {s.Winner} | Ходов: {s.Turns}");
            }

            Console.Write("\nВведите номер сохранения (0 - назад): ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n < 0 || n > saves.Count)
            {
                Console.WriteLine("Неверный ввод. Enter...");
                Console.ReadLine();
                return;
            }

            if (n == 0) return;

            var chosen = saves[n - 1];
            var save = saveService.Load(chosen.FileName);

            Console.Clear();
            Console.WriteLine($"=== Бой из файла: {chosen.FileName} ===");
            Console.WriteLine($"Сохранено (UTC): {save.SavedAtUtc:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Победитель: {save.Winner}");
            Console.WriteLine($"Ходов: {save.Turns}");

            foreach (var line in save.LogLines)
                Console.WriteLine(line);

            Console.WriteLine("Нажмите Enter для возврата в меню...");
            Console.ReadLine();
        }
    }
}