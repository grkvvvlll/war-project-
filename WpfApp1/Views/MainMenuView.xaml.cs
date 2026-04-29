using System.Windows.Controls;

namespace WpfPresentation.Views
{
    public partial class MainMenuView : UserControl
    {
        public event Action? NewGameRequested;
        public event Action? LoadGameRequested;
        public event Action? HelpRequested;
        public event Action? ObserversRequested;
        public event Action? ExitRequested;

        public MainMenuView()
        {
            InitializeComponent();

            NewGameButton.Click += (_, _) => NewGameRequested?.Invoke();
            LoadGameButton.Click += (_, _) => LoadGameRequested?.Invoke();
            HelpButton.Click += (_, _) => HelpRequested?.Invoke();
            ObserversButton.Click += (_, _) => ObserversRequested?.Invoke();
            ExitButton.Click += (_, _) => ExitRequested?.Invoke();
        }
    }
}
