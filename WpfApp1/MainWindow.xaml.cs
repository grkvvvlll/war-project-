using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.Formations;
using Core.Interfaces;
using Services.Observers;
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
            RenumberUnitsFromFront(_army1!, isArmy1: true, _selectedFormation!);
            RenumberUnitsFromFront(_army2!, isArmy1: false, _selectedFormation!);

            _engine = new WpfBattleEngine(_army1!, _army2!, _selectedFormation!);
            _battleView = new BattleView(_army1!, _army2!, _selectedFormation!);
            _battleView.NextRoundRequested += OnNextRound;
            _battleView.UndoRequested += OnUndo;
            _battleView.RedoRequested += OnRedo;
            _battleView.ResetRequested += OnReset;
            _battleView.ArmyCompositionRequested += ShowArmyComposition;
            _battleView.AutoModeRequested += OnAutoMode;
            _battleView.ExitRequested += ShowMainMenu;
            _battleView.FormationChangeRequested += (formation) =>
            {
                _engine.ChangeFormationCommand(formation);
                SyncBattleView();
            };
            MainContent.Content = _battleView;
            SyncBattleView();
        }

        private async void OnNextRound()
        {
            if (_engine == null || _battleView == null) return;

            _battleView.ClearLog();
            _battleView.NextRoundButton.IsEnabled = false;

            var events = _engine.ExecuteRoundCommand();

            foreach (var e in events)
            {
                _battleView.LogEvent(e);

                switch (e.Type)
                {
                    case BattleEventType.MeleeHit:
                    case BattleEventType.MeleeMiss:
                        _battleView.PlayAttack(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        _battleView.PlayHit(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(850);
                        break;

                    case BattleEventType.ArrowShot:
                        // Лучник тянет лук, стрела летит 500 мс, потом вспышка попадания
                        _battleView.PlayShoot(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        _battleView.PlayArrow(
                            e.ActorIsArmy1, e.ActorIndex, e.ActorName,
                            e.TargetIsArmy1, e.TargetIndex, e.TargetName,
                            flightMs: 500);
                        await Task.Delay(500);
                        _battleView.PlayHit(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(450);
                        break;

                    case BattleEventType.Heal:
                        _battleView.PlayHeal(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        _battleView.PlayHealTarget(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(800);
                        break;

                    case BattleEventType.BuffAdded:
                        _battleView.PlayBuffAdded(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        await Task.Delay(700);
                        _battleView.DrawBattlefield();
                        break;

                    case BattleEventType.Spell when e.Message == "Клонирование":
                        _battleView.PlaySpell(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        await Task.Delay(650);
                        _battleView.PlayCloneFlash(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        await Task.Delay(800);
                        _battleView.DrawBattlefield();
                        _battleView.PlayCloneAppear(e.ActorIsArmy1, e.TargetName + " (клон)");
                        await Task.Delay(700);
                        break;

                    case BattleEventType.Spell:
                        _battleView.PlaySpell(e.ActorIsArmy1, e.ActorIndex, e.ActorName);
                        await Task.Delay(600);
                        break;

                    case BattleEventType.Death:
                        _battleView.PlayDeath(e.TargetIsArmy1, e.TargetIndex, e.TargetName);
                        _battleView.DrawBattlefield();
                        break;

                    case BattleEventType.RoundEnd:
                        _battleView.UpdateScore(e.Score1, e.Score2);
                        _battleView.UpdateRound(e.Round + 1);
                        break;

                    case BattleEventType.BattleEnd:
                        RenumberUnitsFromFront(_army1!, isArmy1: true, _selectedFormation!);
                        RenumberUnitsFromFront(_army2!, isArmy1: false, _selectedFormation!);
                        _battleView.DrawBattlefield();
                        MessageBox.Show($"Победитель: {e.Winner}\nСчёт: {e.Score1} : {e.Score2}");
                        ShowMainMenu();
                        return;
                }
            }

            // Перенумерация с фронта в конце каждого раунда
            RenumberUnitsFromFront(_army1!, isArmy1: true, _selectedFormation!);
            RenumberUnitsFromFront(_army2!, isArmy1: false, _selectedFormation!);
            _battleView.DrawBattlefield();
            _battleView.NextRoundButton.IsEnabled = true;
            SyncBattleView();
        }

        private void OnUndo()
        {
            if (_engine == null || _battleView == null) return;
            _engine.UndoCommand();
            SyncBattleView();
        }

        private void OnRedo()
        {
            if (_engine == null || _battleView == null) return;
            _engine.RedoCommand();
            SyncBattleView();
        }

        private void OnReset()
        {
            if (_engine == null || _battleView == null) return;
            _engine.ResetToInitialStateCommand();
            _battleView.ClearLog();
            SyncBattleView();
        }

        private async void OnAutoMode()
        {
            if (_engine == null || _battleView == null) return;

            SetBattleButtonsEnabled(false);
            _battleView.ClearLog();

            while (!_engine.IsOver)
            {
                var events = _engine.ExecuteRoundCommand();
                foreach (var e in events)
                    _battleView.LogEvent(e);

                SyncBattleView();
                SetBattleButtonsEnabled(false);

                var end = events.FirstOrDefault(e => e.Type == BattleEventType.BattleEnd);
                if (end != null)
                {
                    MessageBox.Show($"Победитель: {end.Winner}\nСчёт: {end.Score1} : {end.Score2}");
                    break;
                }

                await Task.Delay(250);
            }

            SetBattleButtonsEnabled(true);
            SyncBattleView();
        }

        private void ShowArmyComposition()
        {
            if (_engine == null) return;

            var text = new StringBuilder();
            AppendArmy(text, _engine.Army1);
            text.AppendLine();
            AppendArmy(text, _engine.Army2);
            MessageBox.Show(text.ToString(), "Состав армий");
        }

        private static void AppendArmy(StringBuilder text, IArmy army)
        {
            text.AppendLine(army.Name);
            foreach (var unit in army.Units)
                text.AppendLine($"  {unit.Name} (HP:{unit.Health}/{unit.MaxHealth}, ATK:{unit.Attack}, DEF:{unit.Defence})");
        }

        private void SetBattleButtonsEnabled(bool isEnabled)
        {
            if (_battleView == null) return;
            _battleView.NextRoundButton.IsEnabled = isEnabled;
            _battleView.UndoButton.IsEnabled = isEnabled && (_engine?.History.CanUndo ?? false);
            _battleView.RedoButton.IsEnabled = isEnabled && (_engine?.History.CanRedo ?? false);
            _battleView.AutoModeButton.IsEnabled = isEnabled;
            _battleView.ArmyCompositionButton.IsEnabled = isEnabled;
            _battleView.ChangeFormationButton.IsEnabled = isEnabled;
            _battleView.ResetButton.IsEnabled = isEnabled;
            _battleView.ExitButton.IsEnabled = isEnabled;
        }

        private void SyncBattleView()
        {
            if (_engine == null || _battleView == null) return;
            _army1 = _engine.Army1;
            _army2 = _engine.Army2;
            _selectedFormation = _engine.Formation;
            _battleView.UpdateState(
                _engine.Army1, _engine.Army2, _engine.Formation,
                _engine.Score1, _engine.Score2, _engine.Round);
            _battleView.UpdateHistory(
                _engine.History.Entries, _engine.History.CanUndo, _engine.History.CanRedo);
        }

        private void ShowLoadGame()
        {
            MessageBox.Show("Загрузить игру — скоро!");
        }

        private void ShowHelp()
        {
            MessageBox.Show("Помощь — скоро!");
        }

        private void ShowObservers()
        {
            var win = new Window
            {
                Title = "Настройки наблюдателей",
                Width = 380,
                Height = 220,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(0x2C, 0x2C, 0x2A))
            };

            var root = new StackPanel { Margin = new Thickness(28) };

            root.Children.Add(new TextBlock
            {
                Text = "Наблюдатели",
                FontFamily = new FontFamily("Georgia"),
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xC7, 0x75)),
                Margin = new Thickness(0, 0, 0, 20)
            });

            root.Children.Add(MakeObserverRow(
                "Звук при гибели юнита",
                "Beep при каждой смерти",
                ObserverRegistry.DeathObserver.IsEnabled,
                v => ObserverRegistry.DeathObserver.IsEnabled = v));

            root.Children.Add(MakeObserverRow(
                "Лог урона в файл",
                "logs/damage-log.txt",
                ObserverRegistry.HealthObserver.IsEnabled,
                v => ObserverRegistry.HealthObserver.IsEnabled = v));

            win.Content = root;
            win.ShowDialog();
        }

        private static UIElement MakeObserverRow(string title, string subtitle,
            bool initial, Action<bool> onChange)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textCol = new StackPanel();
            textCol.Children.Add(new TextBlock
            {
                Text = title,
                FontFamily = new FontFamily("Georgia"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xD1, 0xC7))
            });
            textCol.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(textCol, 0);
            row.Children.Add(textCol);

            bool state = initial;

            var btn = new Button
            {
                Width = 72,
                Height = 30,
                FontFamily = new FontFamily("Georgia"),
                FontSize = 12,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1),
                Background = Brushes.Transparent
            };
            UpdateToggleButton(btn, state);
            Grid.SetColumn(btn, 1);

            btn.Click += (_, _) =>
            {
                state = !state;
                onChange(state);
                UpdateToggleButton(btn, state);
            };

            row.Children.Add(btn);
            return row;
        }

        private static void UpdateToggleButton(Button btn, bool state)
        {
            btn.Content = state ? "ВКЛ" : "ВЫКЛ";
            btn.Foreground = state
                ? new SolidColorBrush(Color.FromRgb(0x1D, 0x9E, 0x75))
                : new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A));
            btn.BorderBrush = state
                ? new SolidColorBrush(Color.FromRgb(0x1D, 0x9E, 0x75))
                : new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x41));
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
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[aliveUnits.Count - 1 - i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
            else
            {
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