using System.Linq;
using System.Windows;
using Core.Interfaces;
using Services.Commands;
using Services.Random;
using Services.Storage;
using Services.UI;
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
        private readonly UnitRenumberer _unitRenumberer = new();
        // Инвокер для команд без undo (навигация, диалоги)
        private readonly SimpleCommandInvoker _simpleInvoker = new();

        private void ShowMainMenu()
        {
            var menu = new MainMenuView();

            // Кнопки главного меню — все через Command (SimpleCommandInvoker)
            menu.NewGameRequested += () => _simpleInvoker.Execute(
                new ActionGameCommand("Новая игра", ShowNewGame, () => { }));

            menu.LoadGameRequested += ShowLoadGame; // исключено из Command 

            menu.HelpRequested += () => _simpleInvoker.Execute(
                new ActionGameCommand("Помощь", ShowHelp, () => { }));

            menu.ExitRequested += () => _simpleInvoker.Execute(
                new ActionGameCommand("Выход", () => Application.Current.Shutdown(), () => { }));

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
            RenumberVisibleArmies();

            _engine = new WpfBattleEngine(_army1!, _army2!, _selectedFormation!);
            _battleView = new BattleView(_army1!, _army2!, _selectedFormation!);
            WireBattleView(_battleView);
            MainContent.Content = _battleView;
            SyncBattleView();
        }
        private void ShowBattleFromSave(BattleResumeData resume)
        {
            _army1 = resume.Army1;
            _army2 = resume.Army2;
            _selectedFormation = resume.Formation;

            _engine = new WpfBattleEngine(resume);
            _battleView = new BattleView(_army1, _army2, _selectedFormation);
            WireBattleView(_battleView);

            MainContent.Content = _battleView;
            SyncBattleView();
        }

        private void WireBattleView(BattleView battleView)
        {
            // Undoable — идут через CommandHistory внутри WpfBattleEngine
            battleView.NextRoundRequested += OnNextRound;
            battleView.UndoRequested += OnUndo;
            battleView.RedoRequested += OnRedo;
            battleView.ResetRequested += OnReset;
            battleView.FormationChangeRequested += OnFormationChangeRequested;

            // Не undoable — идут через SimpleCommandInvoker
            battleView.ArmyCompositionRequested += OnArmyComposition;
            battleView.AutoModeRequested += OnAutoModeCommand;
            battleView.ExitRequested += OnExitBattle;

            // Исключено из Command 
            battleView.SaveRequested += OnSave;
        }

        private void OnFormationChangeRequested(IBattleFormation formation)
        {
            if (_engine == null) return;

            _engine.ChangeFormationCommand(formation);
            SyncBattleView();
        }

        private void OnSave()
        {
            if (_engine == null || _battleView == null) return;

            var dialog = new SaveGameWindow(this);
            if (dialog.ShowDialog() != true) return;

            try
            {
                var save = BattleSaveService.Instance.CreateInProgressSave(
                    _engine.Army1,
                    _engine.Army2,
                    _engine.Round,
                    _engine.Army1TurnState,
                    _engine.Score1,
                    _engine.Score2,
                    _engine.Formation,
                    Enumerable.Empty<string>(),
                    dialog.SaveName);

                BattleSaveService.Instance.Save(save);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                        RenumberVisibleArmies();
                        _battleView.DrawBattlefield();
                        ShowBattleResult(e.Winner ?? "", e.Score1, e.Score2, _engine.Round);
                        return;
                }
            }

            // Перенумерация с фронта в конце каждого раунда
            RenumberVisibleArmies();
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
                    ShowBattleResult(end.Winner ?? "", end.Score1, end.Score2, _engine.Round);
                    return;
                }

                await Task.Delay(250);
            }

            SetBattleButtonsEnabled(true);
            SyncBattleView();
        }

        private void OnArmyComposition()
        {
            if (_engine == null) return;
            _simpleInvoker.Execute(new ActionGameCommand(
                "Состав армий",
                execute: () =>
                {
                    var window = new ArmyCompositionWindow(_engine.Army1, _engine.Army2, this);
                    window.ShowDialog();
                },
                undo: () => { })); // показ диалога не отменяется
        }

        private void OnExitBattle()
        {
            _simpleInvoker.Execute(new ActionGameCommand(
                "Выход в главное меню",
                execute: ShowMainMenu,
                undo: () => { })); // навигация не отменяется
        }

        private void OnAutoModeCommand()
        {
            _simpleInvoker.Execute(new ActionGameCommand(
                "Авторежим",
                execute: OnAutoMode,   
                undo: () => { }));
        }

        private void ShowBattleResult(string winner, int score1, int score2, int rounds)
        {
            var win = new BattleResultWindow(this, winner, score1, score2,
                                             _engine!.Army1, _engine.Army2, rounds);
            win.ShowDialog();
            ShowMainMenu();
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
            var dialog = new LoadGameWindow(this);
            if (dialog.ShowDialog() != true || dialog.SelectedSave == null) return;

            try
            {
                var save = BattleSaveService.Instance.Load(dialog.SelectedSave.FileName);
                var resume = BattleSaveService.Instance.RestoreBattle(save, new RandomService());
                ShowBattleFromSave(resume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить сохранение:\n{ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowHelp()
        {
            var win = new HelpWindow(this);
            win.ShowDialog();
        }

        private void RenumberVisibleArmies()
        {
            if (_army1 == null || _army2 == null || _selectedFormation == null) return;

            _unitRenumberer.Renumber(_army1, isArmy1: true, _selectedFormation);
            _unitRenumberer.Renumber(_army2, isArmy1: false, _selectedFormation);
        }
    }
}
