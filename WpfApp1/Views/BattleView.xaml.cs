using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Formations;
using Core.Interfaces;
using WpfPresentation.Engine;
using WpfPresentation.Views.Units;

namespace WpfPresentation.Views
{
    public partial class BattleView : UserControl
    {
        public event Action? NextRoundRequested;
        public event Action? ExitRequested;
        public event Action<IBattleFormation>? FormationChangeRequested;

        private IArmy _army1;
        private IArmy _army2;
        private IBattleFormation _formation;
        private int _round = 1;
        private int _score1 = 0;
        private int _score2 = 0;

        // Основная карта: (isArmy1, unitIndex) → контрол
        private readonly Dictionary<(bool isArmy1, int index), UserControl> _unitControls = new();

        // Запасной поиск по имени — нужен после перерисовки внутри раунда,
        // когда индексы сдвигаются после RemoveDeadUnits
        private readonly Dictionary<(bool isArmy1, string name), UserControl> _byName = new();

        private const double UnitWidth = 52;
        private const double UnitHeight = 76;
        private const double UnitGapX = 12;
        private const double UnitGapY = 12;
        private const double ArmyGap = 60;

        public BattleView(IArmy army1, IArmy army2, IBattleFormation formation)
        {
            InitializeComponent();
            _army1 = army1;
            _army2 = army2;
            _formation = formation;
            Army1NameText.Text = army1.Name;
            Army2NameText.Text = army2.Name;
            SetupButtons();
            DrawBattlefield();
        }

        private void SetupButtons()
        {
            NextRoundButton.Click += (_, _) => NextRoundRequested?.Invoke();
            ExitButton.Click += (_, _) => ExitRequested?.Invoke();

            ChangeFormationButton.Click += (_, _) =>
            {
                FormationPanel.Visibility = FormationPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            };
            CancelFormationButton.Click += (_, _) => FormationPanel.Visibility = Visibility.Collapsed;
            Formation1Button.Click += (_, _) => { FormationPanel.Visibility = Visibility.Collapsed; FormationChangeRequested?.Invoke(new BridgeFormation()); };
            Formation2Button.Click += (_, _) => { FormationPanel.Visibility = Visibility.Collapsed; FormationChangeRequested?.Invoke(new WideBridgeFormation()); };
            Formation3Button.Click += (_, _) => { FormationPanel.Visibility = Visibility.Collapsed; FormationChangeRequested?.Invoke(new WallFormation()); };

            AutoModeButton.Click += (_, _) => { /* TODO */ };
            ArmyCompositionButton.Click += (_, _) => { /* TODO */ };
        }

        // ── Отрисовка поля боя ────────────────────────────────────────────────
        public void DrawBattlefield()
        {
            BattleCanvas.Children.Clear();
            _unitControls.Clear();
            _byName.Clear();

            var map1 = GetPositionMap(_army1, isArmy1: true);
            var map2 = GetPositionMap(_army2, isArmy1: false);

            int maxCols1 = map1.Any() ? map1.Values.Max(p => p.col) + 1 : 0;
            int maxCols2 = map2.Any() ? map2.Values.Max(p => p.col) + 1 : 0;
            int maxRows = Math.Max(
                map1.Any() ? map1.Values.Max(p => p.row) + 1 : 1,
                map2.Any() ? map2.Values.Max(p => p.row) + 1 : 1);

            double army1Width = maxCols1 * (UnitWidth + UnitGapX);
            double army2Width = maxCols2 * (UnitWidth + UnitGapX);
            double totalWidth = army1Width + ArmyGap + army2Width;
            double totalHeight = maxRows * (UnitHeight + UnitGapY);

            BattleCanvas.Width = totalWidth;
            BattleCanvas.Height = totalHeight;

            // Армия 1 — фронт справа
            foreach (var (unitIndex, (row, col)) in map1)
            {
                double x = army1Width - (col + 1) * (UnitWidth + UnitGapX);
                double y = row * (UnitHeight + UnitGapY);
                var unit = _army1.Units[unitIndex];
                var ctrl = CreateUnitControl(unit, isArmy1: true);
                Place(ctrl, x, y);
                _unitControls[(true, unitIndex)] = ctrl;
                _byName[(true, unit.Name)] = ctrl;
            }

            // Разделительная линия
            BattleCanvas.Children.Add(new Line
            {
                X1 = army1Width + ArmyGap / 2,
                Y1 = 0,
                X2 = army1Width + ArmyGap / 2,
                Y2 = totalHeight,
                Stroke = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x38)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            });

            // Армия 2 — фронт слева
            foreach (var (unitIndex, (row, col)) in map2)
            {
                double x = army1Width + ArmyGap + col * (UnitWidth + UnitGapX);
                double y = row * (UnitHeight + UnitGapY);
                var unit = _army2.Units[unitIndex];
                var ctrl = CreateUnitControl(unit, isArmy1: false);
                Place(ctrl, x, y);
                _unitControls[(false, unitIndex)] = ctrl;
                _byName[(false, unit.Name)] = ctrl;
            }
        }

        private void Place(UserControl ctrl, double x, double y)
        {
            Canvas.SetLeft(ctrl, x);
            Canvas.SetTop(ctrl, y);
            BattleCanvas.Children.Add(ctrl);
        }

        // ── Поиск контрола (по индексу, fallback по имени) ───────────────────
        private UserControl? FindControl(bool isArmy1, int index, string name)
        {
            if (_unitControls.TryGetValue((isArmy1, index), out var ctrl)) return ctrl;
            // После перерисовки внутри раунда индексы сдвигаются — ищем по имени
            return _byName.TryGetValue((isArmy1, name), out ctrl) ? ctrl : null;
        }

        // ── Создание контрола юнита ───────────────────────────────────────────
        private UserControl CreateUnitControl(IUnit unit, bool isArmy1)
        {
            IUnit current = unit;
            while (current is UnitDecorator dec) current = dec.GetInnerUnit();

            UserControl ctrl = current switch
            {
                HeavyUnit => new HeavyUnitControl { IsArmy1 = isArmy1 },
                LightUnit => new LightUnitControl { IsArmy1 = isArmy1 },
                Archer => new ArcherUnitControl { IsArmy1 = isArmy1 },
                Healer => new HealerUnitControl { IsArmy1 = isArmy1 },
                Wizard => new WizardUnitControl { IsArmy1 = isArmy1 },
                GulyayGorodAdapter => new GulyayGorodControl { IsArmy1 = isArmy1 },
                _ => new LightUnitControl { IsArmy1 = isArmy1 }
            };
            ctrl.Width = UnitWidth;
            ctrl.Height = UnitHeight;
            return ctrl;
        }

        // ── Карты позиций ─────────────────────────────────────────────────────
        private Dictionary<int, (int row, int col)> GetPositionMap(IArmy army, bool isArmy1)
        {
            return _formation switch
            {
                WideBridgeFormation wbf => wbf.GetAlivePositionMap(army, isArmy1),
                WallFormation wf => wf.GetAlivePositionMap(army),
                _ => GetBridgePositionMap(army, isArmy1)
            };
        }

        private Dictionary<int, (int row, int col)> GetBridgePositionMap(IArmy army, bool isArmy1)
        {
            var result = new Dictionary<int, (int, int)>();
            var alive = isArmy1
                ? Enumerable.Range(0, army.Units.Count).Where(i => army.Units[i].IsAlive).Reverse().ToList()
                : Enumerable.Range(0, army.Units.Count).Where(i => army.Units[i].IsAlive).ToList();

            for (int slot = 0; slot < alive.Count; slot++)
                result[alive[slot]] = (0, slot);

            return result;
        }

        // ── Обновление состояния ──────────────────────────────────────────────
        public void UpdateScore(int score1, int score2)
        {
            _score1 = score1; _score2 = score2;
            Score1Text.Text = score1.ToString();
            Score2Text.Text = score2.ToString();
        }

        public void UpdateRound(int round)
        {
            _round = round;
            RoundText.Text = $"РАУНД {round}";
        }

        public void UpdateFormation(IBattleFormation formation)
        {
            _formation = formation;
            DrawBattlefield();
        }

        // ── Анимации ──────────────────────────────────────────────────────────

        public void PlayAttack(bool isArmy1, int unitIndex, string unitName = "")
        {
            var ctrl = FindControl(isArmy1, unitIndex, unitName);
            if (ctrl == null) return;

            var translate = new TranslateTransform();
            ctrl.RenderTransform = translate;
            double dir = isArmy1 ? 15 : -15;

            var sb = new Storyboard();
            var anim = new DoubleAnimation { From = 0, To = dir, Duration = TimeSpan.FromMilliseconds(200), AutoReverse = true };
            Storyboard.SetTarget(anim, ctrl);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            sb.Children.Add(anim);
            sb.Begin();
        }

        public void PlayHit(bool isArmy1, int unitIndex, string unitName = "")
        {
            var ctrl = FindControl(isArmy1, unitIndex, unitName);
            if (ctrl == null) return;
            ShowOverlay(ctrl, Color.FromArgb(180, 220, 50, 50), 480);
        }

        /// <summary>Зелёная вспышка на юните, которого вылечили.</summary>
        public void PlayHealTarget(bool isArmy1, int unitIndex, string unitName = "")
        {
            var ctrl = FindControl(isArmy1, unitIndex, unitName);
            if (ctrl == null) return;
            ShowOverlay(ctrl, Color.FromArgb(160, 30, 200, 100), 600);
        }

        // Универсальный цветной оверлей поверх юнита
        private void ShowOverlay(UserControl ctrl, Color color, int durationMs)
        {
            var overlay = new Rectangle
            {
                Width = ctrl.Width,
                Height = ctrl.Height,
                Fill = new SolidColorBrush(color),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(overlay, Canvas.GetLeft(ctrl));
            Canvas.SetTop(overlay, Canvas.GetTop(ctrl));
            BattleCanvas.Children.Add(overlay);

            var sb = new Storyboard();
            var fade = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(durationMs) };
            fade.Completed += (_, _) => BattleCanvas.Children.Remove(overlay);
            Storyboard.SetTarget(fade, overlay);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(fade);
            sb.Begin();
        }

        public void PlayDeath(bool isArmy1, int unitIndex, string unitName = "", Action? onComplete = null)
        {
            var ctrl = FindControl(isArmy1, unitIndex, unitName);
            if (ctrl == null) { onComplete?.Invoke(); return; }

            switch (ctrl)
            {
                case LightUnitControl l: l.PlayDeath(onComplete); break;
                case HeavyUnitControl h: h.PlayDeath(onComplete); break;
                case ArcherUnitControl a: a.PlayDeath(onComplete); break;
                case HealerUnitControl hl: hl.PlayDeath(onComplete); break;
                case WizardUnitControl w: w.PlayDeath(onComplete); break;
                case GulyayGorodControl g: g.PlayDeath(onComplete); break;
                default: onComplete?.Invoke(); break;
            }
        }

        public void PlayShoot(bool isArmy1, int unitIndex, string unitName = "")
        {
            if (FindControl(isArmy1, unitIndex, unitName) is ArcherUnitControl archer)
                archer.PlayShoot();
        }

        public void PlayHeal(bool isArmy1, int unitIndex, string unitName = "")
        {
            if (FindControl(isArmy1, unitIndex, unitName) is HealerUnitControl healer)
                healer.PlayHeal();
        }

        public void PlaySpell(bool isArmy1, int unitIndex, string unitName = "")
        {
            if (FindControl(isArmy1, unitIndex, unitName) is WizardUnitControl wizard)
                wizard.PlaySpell();
        }

        /// <summary>
        /// Белая вспышка в позиции мага — визуальный старт клонирования.
        /// Вызывать до DrawBattlefield(), пока индексы ещё актуальны.
        /// </summary>
        public void PlayCloneFlash(bool isArmy1, int unitIndex, string unitName)
        {
            var ctrl = FindControl(isArmy1, unitIndex, unitName);
            if (ctrl == null) return;

            double cx = Canvas.GetLeft(ctrl) + UnitWidth / 2;
            double cy = Canvas.GetTop(ctrl) + UnitHeight / 2;

            var flash = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
                IsHitTestVisible = false,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };
            Canvas.SetLeft(flash, cx - 4);
            Canvas.SetTop(flash, cy - 4);
            BattleCanvas.Children.Add(flash);

            var sb = new Storyboard();

            void ScaleAnim(string prop)
            {
                var a = new DoubleAnimation
                {
                    From = 1,
                    To = 14,
                    Duration = TimeSpan.FromMilliseconds(550),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(a, flash);
                Storyboard.SetTargetProperty(a, new PropertyPath($"(UIElement.RenderTransform).({prop})"));
                sb.Children.Add(a);
            }
            ScaleAnim("ScaleTransform.ScaleX");
            ScaleAnim("ScaleTransform.ScaleY");

            var fadeOut = new DoubleAnimation
            {
                From = 0.9,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(550),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) => BattleCanvas.Children.Remove(flash);
            Storyboard.SetTarget(fadeOut, flash);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(fadeOut);

            sb.Begin();
        }

        /// <summary>
        /// Появление клона: ищет контрол по имени после DrawBattlefield() и плавно проявляет его.
        /// </summary>
        public void PlayCloneAppear(bool isArmy1, string cloneName)
        {
            if (!_byName.TryGetValue((isArmy1, cloneName), out var ctrl)) return;

            ctrl.Opacity = 0;
            var sb = new Storyboard();
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(450),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fade, ctrl);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(fade);
            sb.Begin();
        }

        // ── Лог событий ───────────────────────────────────────────────────────
        public void LogEvent(BattleEvent e)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            switch (e.Type)
            {
                case BattleEventType.MeleeHit:
                case BattleEventType.MeleeMiss:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " атакует ", "#5F5E5A");
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    if (e.Damage > 0)
                    {
                        AddText(panel, $" → -{e.Damage} HP", "#E24B4A");
                        AddText(panel, $" ({e.HpBefore}→{e.HpAfter})", "#444441");
                    }
                    else AddText(panel, " → 0 урона", "#5F5E5A");
                    break;

                case BattleEventType.ArrowShot:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " стреляет → ", "#5F5E5A");
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, $" -{e.Damage} HP", "#E24B4A");
                    AddText(panel, $" ({e.HpBefore}→{e.HpAfter})", "#444441");
                    break;

                case BattleEventType.ArrowMiss:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " стреляет — стрела не долетает", "#5F5E5A");
                    break;

                case BattleEventType.Heal:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " лечит ", "#5F5E5A");
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, $" +{e.Damage} HP", "#1D9E75");
                    AddText(panel, $" ({e.HpBefore}→{e.HpAfter})", "#444441");
                    break;

                case BattleEventType.HealNoEffect:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " — никого не лечит", "#5F5E5A");
                    break;

                case BattleEventType.Death:
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " погиб", "#E24B4A");
                    break;

                case BattleEventType.BuffLost:
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " потерял бафф ", "#5F5E5A");
                    AddText(panel, e.Message, "#EF9F27");
                    break;

                case BattleEventType.BuffAdded:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, " надел на ", "#5F5E5A");
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, $" {e.Message}", "#EF9F27");
                    break;

                case BattleEventType.Spell:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    if (e.Message == "Клонирование")
                    {
                        AddText(panel, " клонирует ", "#5F5E5A");
                        AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    }
                    else AddText(panel, " использует заклинание", "#7F77DD");
                    break;

                case BattleEventType.RoundEnd:
                    LogPanel.Children.Add(new TextBlock
                    {
                        Text = $"— ХОД {e.Round} —",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A)),
                        Margin = new Thickness(0, 6, 0, 2)
                    });
                    LogScroller.ScrollToBottom();
                    return;

                default:
                    return;
            }

            LogPanel.Children.Add(panel);
            LogScroller.ScrollToBottom();
        }

        private void AddText(StackPanel panel, string text, string hexColor)
        {
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            panel.Children.Add(new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Georgia"),
                FontSize = 14,
                Foreground = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        public void ClearLog() => LogPanel.Children.Clear();
    }
}