using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.Factories.Armies;
using Core.Factories.Units;
using Core.Interfaces;
using Services.Random;

namespace WpfPresentation.Views
{
    public partial class ArmySetupView : UserControl
    {
        public event Action? BackRequested;
        public event Action<IArmy>? ArmyConfirmed;

        private readonly string _armyName;
        private readonly int _budget;
        private int _remaining;
        private bool _isManualMode = false;
        private readonly List<string> _unitChoices = new();

        private static readonly SolidColorBrush BorderActive = new(Color.FromRgb(0x88, 0x87, 0x80));
        private static readonly SolidColorBrush BorderMuted = new(Color.FromRgb(0x5F, 0x5E, 0x5A));
        private static readonly SolidColorBrush BgSelected = new(Color.FromArgb(0x1A, 0xFA, 0xC7, 0x75));
        private static readonly SolidColorBrush GoldBrush = new(Color.FromRgb(0xFA, 0xC7, 0x75));
        private static readonly SolidColorBrush MutedBrush = new(Color.FromRgb(0x88, 0x87, 0x80));

        private readonly Dictionary<string, int> _costs = new()
        {
            { "Heavy", 50 },
            { "Light", 30 },
            { "Archer", 40 },
            { "Healer", 50 },
            { "Wizard", 60 },
            { "GulyayGorod", 70 }
        };

        private readonly Dictionary<string, string> _symbols = new()
        {
            { "Heavy", "⬡" },
            { "Light", "✕" },
            { "Archer", "↑" },
            { "Healer", "✚" },
            { "Wizard", "✶" },
            { "GulyayGorod", "▦" }
        };

        private readonly Dictionary<string, UnitCreator> _unitCreators;
        private readonly AutoArmyFactory _autoFactory;
        private readonly ManualArmyFactory _manualFactory;

        public ArmySetupView(string armyName, int budget)
        {
            InitializeComponent();

            _armyName = armyName;
            _budget = budget;
            _remaining = budget;

            var random = new RandomService();
            _unitCreators = new Dictionary<string, UnitCreator>
            {
                { "Heavy", new HeavyUnitCreator() },
                { "Light", new LightUnitCreator(random) },
                { "Archer", new ArcherUnitCreator() },
                { "Healer", new HealerUnitCreator() },
                { "Wizard", new WizardUnitCreator(random) },
                { "GulyayGorod", new GulyayGorodCreator() }
            };
            _autoFactory = new AutoArmyFactory(_unitCreators, random);
            _manualFactory = new ManualArmyFactory(_unitCreators);

            TitleText.Text = armyName;
            BudgetText.Text = budget.ToString();
            RemainingText.Text = budget.ToString();

            BackButton.Click += (_, _) => BackRequested?.Invoke();
            NextButton.Click += (_, _) => OnNext();
        }

        private void AutoMode_Click(object sender, MouseButtonEventArgs e)
        {
            _isManualMode = false;
            ManualPanel.Visibility = Visibility.Collapsed;

            AutoBorder.BorderBrush = BorderActive;
            AutoBorder.Background = BgSelected;
            AutoText.Foreground = GoldBrush;

            ManualBorder.BorderBrush = BorderMuted;
            ManualBorder.Background = Brushes.Transparent;
            ManualText.Foreground = MutedBrush;
        }

        private void ManualMode_Click(object sender, MouseButtonEventArgs e)
        {
            _isManualMode = true;
            ManualPanel.Visibility = Visibility.Visible;

            ManualBorder.BorderBrush = BorderActive;
            ManualBorder.Background = BgSelected;
            ManualText.Foreground = GoldBrush;

            AutoBorder.BorderBrush = BorderMuted;
            AutoBorder.Background = Brushes.Transparent;
            AutoText.Foreground = MutedBrush;
        }

        private void Heavy_Click(object sender, MouseButtonEventArgs e) => AddUnit("Heavy");
        private void Light_Click(object sender, MouseButtonEventArgs e) => AddUnit("Light");
        private void Archer_Click(object sender, MouseButtonEventArgs e) => AddUnit("Archer");
        private void Healer_Click(object sender, MouseButtonEventArgs e) => AddUnit("Healer");
        private void Wizard_Click(object sender, MouseButtonEventArgs e) => AddUnit("Wizard");
        private void GulyayGorod_Click(object sender, MouseButtonEventArgs e) => AddUnit("GulyayGorod");

        private void AddUnit(string unitType)
        {
            int cost = _costs[unitType];
            if (_remaining < cost)
            {
                ErrorText.Text = "Недостаточно монет!";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;
            _remaining -= cost;
            _unitChoices.Add(unitType);
            RemainingText.Text = _remaining.ToString();
            UpdateComposition();
        }

        private void UpdateComposition()
        {
            if (_unitChoices.Count == 0)
            {
                ArmyCompositionText.Text = "— пусто —";
                return;
            }

            ArmyCompositionText.Text = string.Join("  ",
                _unitChoices.Select(u => $"{_symbols[u]} {u}"));
        }

        private void OnNext()
        {
            IArmy army;

            if (_isManualMode)
            {
                if (_unitChoices.Count == 0)
                {
                    ErrorText.Text = "Добавьте хотя бы одного юнита.";
                    ErrorText.Visibility = Visibility.Visible;
                    return;
                }
                army = _manualFactory.CreateArmy(_armyName, _budget, _unitChoices);
            }
            else
            {
                army = _autoFactory.CreateArmy(_armyName, _budget);
            }

            ArmyConfirmed?.Invoke(army);
        }
    }
}