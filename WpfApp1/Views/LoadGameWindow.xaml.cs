using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Services.Storage;

namespace WpfPresentation.Views
{
    public partial class LoadGameWindow
    {
        public BattleSaveInfo? SelectedSave { get; private set; }

        private readonly BattleSaveService _saveService;

        public LoadGameWindow(Window owner, BattleSaveService saveService)
        {
            InitializeComponent();
            Owner = owner;
            _saveService = saveService;
            PopulateList();
        }

        private void PopulateList()
        {
            var saves = _saveService.ListSaves();

            if (saves.Count == 0)
            {
                SavesList.Visibility = Visibility.Collapsed;
                EmptyPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            foreach (var info in saves)
                SavesList.Items.Add(CreateSaveItem(info));
        }

        private static UIElement CreateSaveItem(BattleSaveInfo info)
        {
            bool isFinished = !string.IsNullOrEmpty(info.Winner);
            var local = info.SavedAtUtc.ToLocalTime();

            var grid = new Grid { Tag = info };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var left = new StackPanel();

            var nameBlock = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(info.DisplayName) ? "Без названия" : info.DisplayName,
                FontFamily = new FontFamily("Georgia"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xD1, 0xC7)),
                Margin = new Thickness(0, 0, 0, 4),
            };

            var metaRow = new StackPanel { Orientation = Orientation.Horizontal };

            void AddMeta(string text, Color color, double rightMargin = 12)
            {
                metaRow.Children.Add(new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Georgia"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(color),
                    Margin = new Thickness(0, 0, rightMargin, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            AddMeta(local.ToString("dd.MM.yyyy  HH:mm"), Color.FromRgb(0x5F, 0x5E, 0x5A));
            AddMeta($"Ход {info.Turns}", Color.FromRgb(0x5F, 0x5E, 0x5A), 0);

            left.Children.Add(nameBlock);
            left.Children.Add(metaRow);
            Grid.SetColumn(left, 0);

            Color badgeColor = isFinished
                ? Color.FromRgb(0x5F, 0x5E, 0x5A)
                : Color.FromRgb(0x1D, 0x9E, 0x75);
            string badgeText = isFinished ? "завершена" : "в процессе";

            var badge = new Border
            {
                BorderBrush = new SolidColorBrush(badgeColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(6, 3, 6, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = badgeText,
                    FontFamily = new FontFamily("Georgia"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(badgeColor),
                }
            };
            Grid.SetColumn(badge, 1);

            grid.Children.Add(left);
            grid.Children.Add(badge);

            return grid;
        }

        private BattleSaveInfo? GetSelectedInfo()
        {
            if (SavesList.SelectedItem is ListBoxItem { Content: UIElement el })
                return (el as FrameworkElement)?.Tag as BattleSaveInfo;

            if (SavesList.SelectedItem is UIElement direct)
                return (direct as FrameworkElement)?.Tag as BattleSaveInfo;

            return null;
        }

        private void SavesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadButton.IsEnabled = SavesList.SelectedIndex >= 0;
        }

        private void SavesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SavesList.SelectedIndex >= 0)
                ConfirmLoad();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e) => ConfirmLoad();

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void ConfirmLoad()
        {
            SelectedSave = GetSelectedInfo();
            if (SelectedSave == null) return;
            DialogResult = true;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}