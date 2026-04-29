using System.Windows;
using Core.Interfaces;
using WpfPresentation.Views;

namespace WpfPresentation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowMainMenu();
        }
        private IBattleFormation? _selectedFormation;

        private void ShowMainMenu()
        {
            var menu = new MainMenuView();
            menu.NewGameRequested += ShowNewGame;
            menu.LoadGameRequested += ShowLoadGame;
            menu.HelpRequested += ShowHelp;
            menu.ObserversRequested += ShowObservers;
            menu.ExitRequested += () => Application.Current.Shutdown();
            MainContent.Content = menu;
        }

        private void ShowNewGame()
        {
            var view = new FormationSelectView();
            view.BackRequested += ShowMainMenu;
            view.FormationSelected += (formation) =>
            {
                _selectedFormation = formation;
                ShowBudget();
            };
            MainContent.Content = view;
        }

        private void ShowBudget()
        {
            var view = new BudgetView();
            view.BackRequested += ShowNewGame;
            view.BudgetConfirmed += (budget) =>
            {
                // TODO: следующий экран — создание армии 1
                MessageBox.Show($"Бюджет: {budget}");
            };
            MainContent.Content = view;
        }

        private void ShowLoadGame()
        {
            // TODO: показать экран загрузки
            MessageBox.Show("Загрузить игру — скоро!");
        }

        private void ShowHelp()
        {
            // TODO: показать экран помощи
            MessageBox.Show("Помощь — скоро!");
        }

        private void ShowObservers()
        {
            // TODO: показать настройки наблюдателей
            MessageBox.Show("Настройки наблюдателей — скоро!");
        }
    }
}
