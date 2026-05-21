using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Services.Observers;

namespace WpfPresentation.Views
{
    public partial class MainMenuView : UserControl
    {
        public event Action? NewGameRequested;
        public event Action? LoadGameRequested;
        public event Action? HelpRequested;
        public event Action? ExitRequested;

        public MainMenuView()
        {
            InitializeComponent();

            NewGameButton.Click += (_, _) => NewGameRequested?.Invoke();
            LoadGameButton.Click += (_, _) => LoadGameRequested?.Invoke();
            HelpButton.Click += (_, _) => HelpRequested?.Invoke();
            ExitButton.Click += (_, _) => ExitRequested?.Invoke();

            // Инициализируем кнопки текущим состоянием наблюдателей
            RefreshToggle(DeathToggle, ObserverRegistry.DeathObserver.IsEnabled);
            RefreshToggle(HealthToggle, ObserverRegistry.HealthObserver.IsEnabled);

            // Раскрыть/свернуть панель
            ObserversButton.Click += (_, _) =>
            {
                ObserversPanel.Visibility = ObserversPanel.Visibility == Visibility.Collapsed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };

            DeathToggle.Click += (_, _) =>
            {
                ObserverRegistry.DeathObserver.IsEnabled = !ObserverRegistry.DeathObserver.IsEnabled;
                RefreshToggle(DeathToggle, ObserverRegistry.DeathObserver.IsEnabled);
            };

            HealthToggle.Click += (_, _) =>
            {
                ObserverRegistry.HealthObserver.IsEnabled = !ObserverRegistry.HealthObserver.IsEnabled;
                RefreshToggle(HealthToggle, ObserverRegistry.HealthObserver.IsEnabled);
            };
        }

        private static void RefreshToggle(Button btn, bool isOn)
        {
            btn.Content = isOn ? "ВКЛ" : "ВЫКЛ";
            btn.Foreground = isOn
                ? new SolidColorBrush(Color.FromRgb(0x1D, 0x9E, 0x75))
                : new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A));
            btn.BorderBrush = isOn
                ? new SolidColorBrush(Color.FromRgb(0x1D, 0x9E, 0x75))
                : new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x41));
        }
    }
}
