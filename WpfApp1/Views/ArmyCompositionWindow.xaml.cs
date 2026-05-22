using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Core.Interfaces;

namespace WpfPresentation.Views
{
    public partial class ArmyCompositionWindow
    {
        public ArmyCompositionWindow(IArmy army1, IArmy army2, Window owner)
        {
            InitializeComponent();
            Owner = owner;
            PopulateArmy(army1, Army1Panel, Army1NameText, Army1StatsText);
            PopulateArmy(army2, Army2Panel, Army2NameText, Army2StatsText);
            FooterText.Text = $"Всего юнитов: {army1.Units.Count + army2.Units.Count}  ·  " +
                              $"Живых: {army1.Units.Count(u => u.IsAlive) + army2.Units.Count(u => u.IsAlive)}";
        }

        private static void PopulateArmy(IArmy army, StackPanel panel,
                                          TextBlock nameText, TextBlock statsText)
        {
            nameText.Text = army.Name;

            int alive = army.Units.Count(u => u.IsAlive);
            int total = army.Units.Count;
            statsText.Text = $"Живых: {alive} / {total}   ·   Стоимость: {army.TotalCost}";

            foreach (var unit in army.Units)
                panel.Children.Add(CreateUnitRow(unit));
        }

        private static UIElement CreateUnitRow(IUnit unit)
        {
            bool alive = unit.IsAlive;
            double hpFraction = unit.MaxHealth > 0
                ? Math.Clamp((double)unit.Health / unit.MaxHealth, 0, 1)
                : 0;

            // Цвет HP-бара: зелёный → жёлтый → красный
            Color barColor = hpFraction > 0.5
                ? Color.FromRgb(0x6B, 0xAD, 0x5E)
                : hpFraction > 0.25
                    ? Color.FromRgb(0xEF, 0x9F, 0x27)
                    : Color.FromRgb(0xC0, 0x3C, 0x3C);

            var outer = new Border
            {
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(8, 6, 8, 6),
                BorderBrush = new SolidColorBrush(alive ? Color.FromRgb(0x44, 0x44, 0x41) : Color.FromRgb(0x33, 0x33, 0x31)),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(alive ? Color.FromRgb(0x30, 0x30, 0x2E) : Color.FromRgb(0x28, 0x28, 0x26)),
                Opacity = alive ? 1.0 : 0.45,
            };

            var stack = new StackPanel();

            // Строка: имя + HP текст
            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = unit.Name,
                FontFamily = new FontFamily("Georgia"),
                FontSize = 12,
                Foreground = new SolidColorBrush(alive ? Color.FromRgb(0xD3, 0xD1, 0xC7) : Color.FromRgb(0x66, 0x65, 0x62)),
                TextDecorations = alive ? null : TextDecorations.Strikethrough,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(nameBlock, 0);

            var hpText = new TextBlock
            {
                Text = alive ? $"{unit.Health} / {unit.MaxHealth}" : "павший",
                FontFamily = new FontFamily("Georgia"),
                FontSize = 10,
                Foreground = new SolidColorBrush(alive
                    ? barColor
                    : Color.FromRgb(0x55, 0x54, 0x51)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Grid.SetColumn(hpText, 1);

            topRow.Children.Add(nameBlock);
            topRow.Children.Add(hpText);
            stack.Children.Add(topRow);

            // HP-бар
            var hpBarOuter = new Border
            {
                Height = 3,
                Margin = new Thickness(0, 4, 0, 5),
                Background = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x38)),
                CornerRadius = new CornerRadius(1),
            };
            var hpBarGrid = new Grid();
            var hpBarFill = new Rectangle
            {
                Fill = new SolidColorBrush(barColor),
                HorizontalAlignment = HorizontalAlignment.Left,
                RadiusX = 1,
                RadiusY = 1,
            };
            // Bind fill width relative to parent via a loaded event
            double fraction = hpFraction;
            hpBarFill.Loaded += (_, _) =>
            {
                hpBarFill.Width = hpBarGrid.ActualWidth * fraction;
            };
            hpBarGrid.SizeChanged += (_, e) =>
            {
                hpBarFill.Width = e.NewSize.Width * fraction;
            };
            hpBarGrid.Children.Add(hpBarFill);
            hpBarOuter.Child = hpBarGrid;
            stack.Children.Add(hpBarOuter);

            // Строка со статами
            if (alive)
            {
                var statsRow = new StackPanel { Orientation = Orientation.Horizontal };

                void AddStat(string label, string value, Color color)
                {
                    statsRow.Children.Add(new TextBlock
                    {
                        Text = label,
                        FontFamily = new FontFamily("Georgia"),
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A)),
                        Margin = new Thickness(0, 0, 2, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                    statsRow.Children.Add(new TextBlock
                    {
                        Text = value,
                        FontFamily = new FontFamily("Georgia"),
                        FontSize = 10,
                        Foreground = new SolidColorBrush(color),
                        Margin = new Thickness(0, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                }

                AddStat("АТК", unit.Attack.ToString(), Color.FromRgb(0xEF, 0x9F, 0x27));
                AddStat("ЗАЩ", unit.Defence.ToString(), Color.FromRgb(0x6B, 0xAD, 0x5E));
                AddStat("ЦНА", unit.Cost.ToString(), Color.FromRgb(0x88, 0x87, 0x80));

                if (unit.SpecialAbility != null)
                {
                    statsRow.Children.Add(new TextBlock
                    {
                        Text = "✦ спец.",
                        FontFamily = new FontFamily("Georgia"),
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x9A, 0xD1)),
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                }

                stack.Children.Add(statsRow);
            }

            outer.Child = stack;
            return outer;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}