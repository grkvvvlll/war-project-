using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Interfaces;

namespace WpfPresentation.Views
{
    public partial class ArmyPreviewView : UserControl
    {
        public event Action? BackRequested;
        public event Action? BattleStartRequested;

        private static readonly Dictionary<string, string> Symbols = new()
        {
            { nameof(HeavyUnit), "⬡" },
            { nameof(LightUnit), "✕" },
            { nameof(Archer),    "↑" },
            { nameof(Healer),    "✚" },
            { nameof(Wizard),    "✶" },
            { nameof(GulyayGorodAdapter), "▦" }
        };

        public ArmyPreviewView(IArmy army1, IArmy army2, int budget)
        {
            InitializeComponent();

            Army1Title.Text = army1.Name;
            Army2Title.Text = army2.Name;
            Army1Budget.Text = $"Бюджет: {budget} монет";
            Army2Budget.Text = $"Бюджет: {budget} монет";

            Army1List.ItemsSource = BuildUnitRows(army1);
            Army2List.ItemsSource = BuildUnitRows(army2);

            BackButton.Click += (_, _) => BackRequested?.Invoke();
            StartBattleButton.Click += (_, _) => BattleStartRequested?.Invoke();
        }

        private List<UIElement> BuildUnitRows(IArmy army)
        {
            var rows = new List<UIElement>();

            foreach (var unit in army.Units)
            {
                string symbol = GetSymbol(unit);

                var grid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var symbolText = new TextBlock
                {
                    Text = symbol,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xB4, 0xB2, 0xA9)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var nameText = new TextBlock
                {
                    Text = unit.Name,
                    FontFamily = new FontFamily("Georgia"),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xD1, 0xC7)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var statsText = new TextBlock
                {
                    Text = $"HP:{unit.Health} ATK:{unit.Attack} DEF:{unit.Defence}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(symbolText, 0);
                Grid.SetColumn(nameText, 1);
                Grid.SetColumn(statsText, 2);

                grid.Children.Add(symbolText);
                grid.Children.Add(nameText);
                grid.Children.Add(statsText);

                rows.Add(grid);
            }

            return rows;
        }

        private string GetSymbol(IUnit unit)
        {
            IUnit current = unit;
            while (current is UnitDecorator dec)
                current = dec.GetInnerUnit();

            return current switch
            {
                HeavyUnit => "⬡",
                LightUnit => "✕",
                Archer => "↑",
                Healer => "✚",
                Wizard => "✶",
                GulyayGorodAdapter => "▦",
                _ => "?"
            };
        }
    }
}