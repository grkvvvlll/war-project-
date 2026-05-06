using System.Windows;
using Core.Formations;
using Core.Interfaces;
using WpfPresentation.Engine;
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
        private BattleView? _battleView;
        private IArmy? _army1;
        private IArmy? _army2;
        private int _budget;
        private WpfBattleEngine? _engine;

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
                _budget = budget;
                ShowArmySetup(1);
            };
            MainContent.Content = view;
        }

        private void ShowArmySetup(int armyNumber)
        {
            var view = new ArmySetupView($"Армия {armyNumber}", _budget);
            view.BackRequested += () =>
            {
                if (armyNumber == 1) ShowBudget();
                else ShowArmySetup(1);
            };
            view.ArmyConfirmed += (army) =>
            {
                if (armyNumber == 1)
                {
                    _army1 = army;
                    ShowArmySetup(2);
                }
                else
                {
                    _army2 = army;
                    ShowArmyPreview();
                }
            };
            MainContent.Content = view;
        }

        private void ShowArmyPreview()
        {
            var view = new ArmyPreviewView(_army1!, _army2!, _budget);
            view.BackRequested += () => ShowArmySetup(2);
            view.BattleStartRequested += ShowBattle;
            MainContent.Content = view;
        }

        private void ShowBattle()
        {
            _engine = new WpfBattleEngine(_army1!, _army2!, _selectedFormation!);
            _battleView = new BattleView(_army1!, _army2!, _selectedFormation!);
            _battleView.NextRoundRequested += OnNextRound;
            _battleView.ExitRequested += ShowMainMenu;
            _battleView.FormationChangeRequested += (formation) =>
            {
                _selectedFormation = formation;
                _engine.SetFormation(formation);
                _battleView.UpdateFormation(formation);
            };
            MainContent.Content = _battleView;
        }

        private async void OnNextRound()
        {
            if (_engine == null || _battleView == null) return;

            _battleView.ClearLog();
            _battleView.NextRoundButton.IsEnabled = false;

            var events = _engine.ExecuteRound();

            foreach (var e in events)
            {
                _battleView.LogEvent(e);

                switch (e.Type)
                {
                    case BattleEventType.MeleeHit:
                    case BattleEventType.MeleeMiss:
                        _battleView.PlayAttack(e.ActorIsArmy1, e.ActorIndex);
                        _battleView.PlayHit(e.TargetIsArmy1, e.TargetIndex);
                        await Task.Delay(300);
                        break;
                    case BattleEventType.ArrowShot:
                        _battleView.PlayShoot(e.ActorIsArmy1, e.ActorIndex);
                        _battleView.PlayHit(e.TargetIsArmy1, e.TargetIndex);
                        await Task.Delay(300);
                        break;
                    case BattleEventType.Heal:
                        _battleView.PlayHeal(e.ActorIsArmy1, e.ActorIndex);
                        await Task.Delay(200);
                        break;
                    case BattleEventType.Spell:
                        _battleView.PlaySpell(e.ActorIsArmy1, e.ActorIndex);
                        await Task.Delay(200);
                        break;
                    case BattleEventType.Death:
                        _battleView.PlayDeath(e.TargetIsArmy1, e.TargetIndex);
                        await Task.Delay(500);
                        break;
                    case BattleEventType.RoundEnd:
                        _battleView.UpdateScore(e.Score1, e.Score2);
                        _battleView.UpdateRound(e.Round + 1);
                        break;
                    case BattleEventType.BattleEnd:
                        _battleView.DrawBattlefield();
                        MessageBox.Show($"Победитель: {e.Winner}\nСчёт: {e.Score1} : {e.Score2}");
                        ShowMainMenu();
                        return;
                }
            }

            _battleView.DrawBattlefield();
            _battleView.NextRoundButton.IsEnabled = true;
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
