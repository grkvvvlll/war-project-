using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Services.Commands;
using Services.Observers;

namespace WpfPresentation.Views
{
    public partial class MainMenuView : UserControl
    {
        public event Action? NewGameRequested;
        public event Action? LoadGameRequested;
        public event Action? HelpRequested;
        public event Action? ExitRequested;

        // Инвокер для команд внутри главного меню
        private readonly SimpleCommandInvoker _invoker = new();

        public MainMenuView()
        {
            InitializeComponent();

            // Навигационные кнопки: события поднимаются наверх в MainWindow,
            // где уже оборачиваются в команды через _simpleInvoker
            NewGameButton.Click += (_, _) => NewGameRequested?.Invoke();
            LoadGameButton.Click += (_, _) => LoadGameRequested?.Invoke(); // исключено
            HelpButton.Click += (_, _) => HelpRequested?.Invoke();
            ExitButton.Click += (_, _) => ExitRequested?.Invoke();

            // Инициализируем тоглы текущим состоянием наблюдателей
            RefreshToggle(DeathToggle, ObserverRegistry.Instance.DeathObserver.IsEnabled);
            RefreshToggle(HealthToggle, ObserverRegistry.Instance.HealthObserver.IsEnabled);

            ObserversButton.Click += (_, _) =>
            {
                ObserversPanel.Visibility = ObserversPanel.Visibility == Visibility.Collapsed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };

            // Переключение наблюдателей — undoable команды
            DeathToggle.Click += (_, _) =>
            {
                bool stateBefore = ObserverRegistry.Instance.DeathObserver.IsEnabled;
                _invoker.Execute(new ActionGameCommand(
                    "Переключить звук смерти",
                    execute: () =>
                    {
                        ObserverRegistry.Instance.DeathObserver.IsEnabled = !stateBefore;
                        RefreshToggle(DeathToggle, ObserverRegistry.Instance.DeathObserver.IsEnabled);
                    },
                    undo: () =>
                    {
                        ObserverRegistry.Instance.DeathObserver.IsEnabled = stateBefore;
                        RefreshToggle(DeathToggle, ObserverRegistry.Instance.DeathObserver.IsEnabled);
                    }));
            };

            HealthToggle.Click += (_, _) =>
            {
                bool stateBefore = ObserverRegistry.Instance.HealthObserver.IsEnabled;
                _invoker.Execute(new ActionGameCommand(
                    "Переключить лог урона",
                    execute: () =>
                    {
                        ObserverRegistry.Instance.HealthObserver.IsEnabled = !stateBefore;
                        RefreshToggle(HealthToggle, ObserverRegistry.Instance.HealthObserver.IsEnabled);
                    },
                    undo: () =>
                    {
                        ObserverRegistry.Instance.HealthObserver.IsEnabled = stateBefore;
                        RefreshToggle(HealthToggle, ObserverRegistry.Instance.HealthObserver.IsEnabled);
                    }));
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