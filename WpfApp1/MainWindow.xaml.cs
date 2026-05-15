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
            // Нумеруем юнитов с фронта — как в консольном слое
            RenumberUnitsFromFront(_army1!, isArmy1: true, _selectedFormation!);
            RenumberUnitsFromFront(_army2!, isArmy1: false, _selectedFormation!);

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
                        _battleView.PlayAttack(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        _battleView.PlayHit(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(550);
                        break;

                    case BattleEventType.ArrowShot:
                        _battleView.PlayShoot(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        _battleView.PlayHit(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(550);
                        break;

                    case BattleEventType.Heal:
                        _battleView.PlayHeal(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        _battleView.PlayHealTarget(e.TargetIsArmy1, e.TargetIndex, e.TargetName); // зелёная подсветка
                        await Task.Delay(500);
                        break;

                    case BattleEventType.Spell when e.Message == "Клонирование":
                        // 1. Маг произносит заклинание
                        _battleView.PlaySpell(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        await Task.Delay(500);
                        // 2. Белая вспышка — магия формирует копию
                        _battleView.PlayCloneFlash(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        await Task.Delay(650);
                        // 3. Перерисовываем — клон уже вставлен в армию движком
                        _battleView.DrawBattlefield();
                        // 4. Плавное появление клона
                        _battleView.PlayCloneAppear(e.ActorIsArmy1, e.TargetName + " (клон)");
                        await Task.Delay(600);
                        break;

                    case BattleEventType.Spell:
                        _battleView.PlaySpell(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        await Task.Delay(400);
                        break;

                    case BattleEventType.Death:
                        _battleView.PlayDeath(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(750);
                        // Сразу перестраиваем строй — мёртвый уже удалён из армии движком
                        _battleView.DrawBattlefield();
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

        /// <summary>
        /// Переименовывает юнитов армии так, чтобы фронтовой юнит всегда был #1.
        /// Логика идентична ConsoleMenu.RenumberUnitsFromFront.
        /// </summary>
        private static void RenumberUnitsFromFront(IArmy army, bool isArmy1, IBattleFormation formation)
        {
            var aliveUnits = army.Units.Where(u => u.IsAlive).ToList();
            bool isWall = formation is WallFormation;

            if (isArmy1 && !isWall)
            {
                // Армия 1 в обычных построениях: фронт — последний в списке → он получает №1
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[aliveUnits.Count - 1 - i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
            else
            {
                // Армия 2 или стенка: фронт — первый в списке → он получает №1
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
        }
    }
}