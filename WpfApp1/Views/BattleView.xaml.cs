using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        public event Action? UndoRequested;
        public event Action? RedoRequested;
        public event Action? ResetRequested;
        public event Action? ArmyCompositionRequested;
        public event Action? AutoModeRequested;
        public event Action? ExitRequested;
        public event Action? SaveRequested;
        public event Action<IBattleFormation>? FormationChangeRequested;

        private IArmy _army1;
        private IArmy _army2;
        private IBattleFormation _formation;
        private int _round = 1;
        private int _score1 = 0;
        private int _score2 = 0;

        private readonly Dictionary<(bool isArmy1, int index), UserControl> _unitControls = new();
        private readonly Dictionary<(bool isArmy1, string name), UserControl> _byName = new();
        private readonly BattleAnimator _animator;
        private readonly BattleEventLogger _eventLogger;

        private const double UnitWidth = 52;
        private const double UnitHeight = 76;
        private const double UnitGapX = 12;
        private const double UnitGapY = 12;
        private const double ArmyGap = 60;

        public BattleView(IArmy army1, IArmy army2, IBattleFormation formation)
        {
            InitializeComponent();
            _animator = new BattleAnimator(
                BattleCanvas, FindControl, _byName, UnitWidth, UnitHeight);
            _eventLogger = new BattleEventLogger(LogPanel, LogScroller);
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
            UndoButton.Click += (_, _) => UndoRequested?.Invoke();
            RedoButton.Click += (_, _) => RedoRequested?.Invoke();
            ResetButton.Click += (_, _) => ResetRequested?.Invoke();
            ExitButton.Click += (_, _) => ExitRequested?.Invoke();
            SaveButton.Click += (_, _) => SaveRequested?.Invoke();

            ChangeFormationButton.Click += (_, _) =>
            {
                FormationPanel.Visibility = FormationPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed : Visibility.Visible;
            };
            CancelFormationButton.Click += (_, _) => FormationPanel.Visibility = Visibility.Collapsed;
            Formation1Button.Click += (_, _) => { FormationPanel.Visibility = Visibility.Collapsed; FormationChangeRequested?.Invoke(new BridgeFormation()); };
            Formation2Button.Click += (_, _) => { FormationPanel.Visibility = Visibility.Collapsed; FormationChangeRequested?.Invoke(new WideBridgeFormation()); };
            Formation3Button.Click += (_, _) => { FormationPanel.Visibility = Visibility.Collapsed; FormationChangeRequested?.Invoke(new WallFormation()); };

            AutoModeButton.Click += (_, _) => AutoModeRequested?.Invoke();
            ArmyCompositionButton.Click += (_, _) => ArmyCompositionRequested?.Invoke();
        }

        // Отрисовка поля боя 
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

            foreach (var (unitIndex, (row, col)) in map1)
            {
                double x = army1Width - (col + 1) * (UnitWidth + UnitGapX);
                double y = row * (UnitHeight + UnitGapY);
                var unit = _army1.Units[unitIndex];
                var ctrl = CreateUnitControl(unit, isArmy1: true);
                Place(ctrl, x, y);
                _unitControls[(true, unitIndex)] = ctrl;
                _byName[(true, unit.Name)] = ctrl;
                DrawBuffOverlays(unit, x, y);
            }

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

            foreach (var (unitIndex, (row, col)) in map2)
            {
                double x = army1Width + ArmyGap + col * (UnitWidth + UnitGapX);
                double y = row * (UnitHeight + UnitGapY);
                var unit = _army2.Units[unitIndex];
                var ctrl = CreateUnitControl(unit, isArmy1: false);
                Place(ctrl, x, y);
                _unitControls[(false, unitIndex)] = ctrl;
                _byName[(false, unit.Name)] = ctrl;
                DrawBuffOverlays(unit, x, y);
            }
        }

        private void Place(UserControl ctrl, double x, double y)
        {
            Canvas.SetLeft(ctrl, x);
            Canvas.SetTop(ctrl, y);
            BattleCanvas.Children.Add(ctrl);
        }

        // Баффы
        private void DrawBuffOverlays(IUnit unit, double x, double y)
        {
            foreach (var buffName in CollectBuffs(unit))
            {
                switch (buffName)
                {
                    case "Шлем": DrawHelmBuff(x, y); break;
                    case "Щит": DrawShieldBuff(x, y); break;
                    case "Копьё": DrawSpearBuff(x, y); break;
                    case "Конь": DrawHorseBuff(x, y); break;
                }
            }
        }

        private static List<string> CollectBuffs(IUnit unit)
        {
            var result = new List<string>();
            IUnit current = unit;
            while (current is UnitDecorator dec)
            {
                if (dec.GetCurrentBuff() is { } b) result.Add(b.NameNominative);
                current = dec.GetInnerUnit();
            }
            result.Reverse();
            return result;
        }

        // Шлем 
        private void DrawHelmBuff(double x, double y)
        {
            var gold = Brush(0xFA, 0xC7, 0x75);
            var goldFill = BrushA(40, 0xFA, 0xC7, 0x75);
            var dark = Brush(0x2C, 0x2C, 0x2A);

            HP("M10,21 L10,2 Q10,0 24,0 Q38,0 38,2 L38,21 Q34,23 24,23 Q14,23 10,21 Z",
               x, y, gold, 1.8, goldFill);

            var slit = new Rectangle
            {
                Width = 24,
                Height = 3,
                Fill = dark,
                RadiusX = 1,
                RadiusY = 1,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(slit, x + 12); Canvas.SetTop(slit, y + 9);
            BattleCanvas.Children.Add(slit);

            HL(x + 24, y + 12, x + 24, y + 20, gold, 1.0);

            foreach (double hx in new[] { 15.5, 19.5, 27.5, 31.5 })
            {
                var dot = new Ellipse { Width = 2, Height = 2, Fill = gold, IsHitTestVisible = false };
                Canvas.SetLeft(dot, x + hx); Canvas.SetTop(dot, y + 15);
                BattleCanvas.Children.Add(dot);
            }

            HL(x + 10, y + 21, x + 38, y + 21, gold, 1.0);
        }

        // Щит 
        private void DrawShieldBuff(double x, double y)
        {
            var blue = Brush(0x7A, 0xB4, 0xE8);
            var blueFill = BrushA(40, 0x7A, 0xB4, 0xE8);

            HP("M-3,20 L-3,40 Q-3,51 7,54 Q17,51 17,40 L17,20 Z",
               x, y, blue, 1.8, blueFill);

            HL(x + 7, y + 22, x + 7, y + 52, blue, 1.0);
            HL(x - 2, y + 35, x + 16, y + 35, blue, 1.0);

            var boss = new Ellipse
            {
                Width = 6,
                Height = 6,
                Stroke = blue,
                StrokeThickness = 1.8,
                Fill = blueFill,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(boss, x + 4); Canvas.SetTop(boss, y + 32);
            BattleCanvas.Children.Add(boss);
        }

        // Копьё 
        private void DrawSpearBuff(double x, double y)
        {
            var bronze = Brush(0xD0, 0x78, 0x30);

            HL(x + 43, y + 62, x + 46, y + 4, bronze, 2.0);

            BattleCanvas.Children.Add(new Polygon
            {
                Points = new PointCollection
                {
                    new(x + 44, y + 4),
                    new(x + 48, y + 4),
                    new(x + 46, y - 2)
                },
                Fill = bronze,
                Stroke = bronze,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            });

            HL(x + 39, y + 17, x + 53, y + 13, bronze, 2.5);
        }

        // Конь 
        private void DrawHorseBuff(double x, double y)
        {
            var brown = Brush(0x9A, 0x70, 0x40);
            var brownFill = BrushA(30, 0x9A, 0x70, 0x40);
            var t = Brushes.Transparent;

            HP("M6,50 Q10,47 20,48 Q32,48 44,52", x, y, brown, 1.8, t); // спина
            HP("M6,50 Q3,55 6,68", x, y, brown, 1.8, t); // круп
            HP("M44,52 Q48,57 44,70", x, y, brown, 1.8, t); // грудь
            HP("M6,68 Q25,74 44,70", x, y, brown, 1.8, t); // живот

            HP("M40,50 Q46,42 44,34 Q43,28 40,26", x, y, brown, 1.8, t);
            HP("M40,26 Q46,24 52,27 Q55,30 53,35 Q51,38 46,38 Q40,37 40,33 Z",
               x, y, brown, 1.8, brownFill);

            HP("M41,26 L39,21 L43,24", x, y, brown, 1.3, t);

            BattleCanvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M41,28 Q38,32 39,38 Q39,44 40,50"),
                Stroke = brown,
                StrokeThickness = 1.4,
                StrokeDashArray = new DoubleCollection { 3, 2.5 },
                Fill = t,
                IsHitTestVisible = false,
                RenderTransform = new TranslateTransform(x, y)
            });

            HP("M38,70 L36,78 Q35,80 33,86", x, y, brown, 1.8, t);
            HP("M43,70 L42,77 Q42,80 41,86", x, y, brown, 1.8, t);

            HP("M10,68 L9,75 Q8,78 11,80 L10,86", x, y, brown, 1.8, t);
            HP("M16,69 L15,75 Q14,78 17,80 L16,86", x, y, brown, 1.8, t);

            HL(x + 30, y + 86, x + 36, y + 86, brown, 2.5);
            HL(x + 38, y + 86, x + 44, y + 86, brown, 2.5);
            HL(x + 7, y + 86, x + 13, y + 86, brown, 2.5);
            HL(x + 13, y + 86, x + 19, y + 86, brown, 2.5);

            HP("M6,52 Q-2,52 -4,60 Q-5,68 0,72", x, y, brown, 2.2, t);
            HP("M6,52 Q-1,56 0,66", x, y, brown, 1.2, t);

            var eye = new Ellipse { Width = 3, Height = 3, Fill = brown, IsHitTestVisible = false };
            Canvas.SetLeft(eye, x + 48.5); Canvas.SetTop(eye, y + 27.5);
            BattleCanvas.Children.Add(eye);

            var nostril = new Ellipse { Width = 2.4, Height = 2, Fill = brown, IsHitTestVisible = false };
            Canvas.SetLeft(nostril, x + 52.8); Canvas.SetTop(nostril, y + 33);
            BattleCanvas.Children.Add(nostril);
        }

        // Анимация при получении баффа 
        private void HP(string data, double dx, double dy,
            SolidColorBrush stroke, double thickness, Brush fill)
        {
            BattleCanvas.Children.Add(new Path
            {
                Data = Geometry.Parse(data),
                Stroke = stroke,
                StrokeThickness = thickness,
                Fill = fill,
                IsHitTestVisible = false,
                RenderTransform = new TranslateTransform(dx, dy)
            });
        }

        private void HL(double x1, double y1, double x2, double y2,
            SolidColorBrush stroke, double thickness)
        {
            BattleCanvas.Children.Add(new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = stroke,
                StrokeThickness = thickness,
                IsHitTestVisible = false
            });
        }

        private static SolidColorBrush Brush(byte r, byte g, byte b) =>
            new(Color.FromRgb(r, g, b));

        private static SolidColorBrush BrushA(byte a, byte r, byte g, byte b) =>
            new(Color.FromArgb(a, r, g, b));

        private UserControl? FindControl(bool isArmy1, int index, string name)
        {
            if (_unitControls.TryGetValue((isArmy1, index), out var ctrl)) return ctrl;
            return _byName.TryGetValue((isArmy1, name), out ctrl) ? ctrl : null;
        }

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

        // Обновление состояния 
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

        public void UpdateState(IArmy army1, IArmy army2, IBattleFormation formation,
            int score1, int score2, int round)
        {
            _army1 = army1; _army2 = army2; _formation = formation;
            Army1NameText.Text = army1.Name;
            Army2NameText.Text = army2.Name;
            UpdateScore(score1, score2);
            UpdateRound(round + 1);
            DrawBattlefield();
        }

        public void UpdateHistory(IEnumerable<string> entries, bool canUndo, bool canRedo)
        {
            UndoButton.IsEnabled = canUndo;
            RedoButton.IsEnabled = canRedo;
        }

        // Анимации 
        public void PlayAttack(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlayAttack(isArmy1, unitIndex, unitName);

        public void PlayHit(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlayHit(isArmy1, unitIndex, unitName);

        public void PlayHealTarget(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlayHealTarget(isArmy1, unitIndex, unitName);

        public void PlayDeath(bool isArmy1, int unitIndex, string unitName = "", Action? onComplete = null) =>
            _animator.PlayDeath(isArmy1, unitIndex, unitName, onComplete);

        public void PlayShoot(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlayShoot(isArmy1, unitIndex, unitName);

        public void PlayArrow(bool archerIsArmy1, int archerIndex, string archerName,
            bool targetIsArmy1, int targetIndex, string targetName, int flightMs = 500) =>
            _animator.PlayArrow(archerIsArmy1, archerIndex, archerName,
                targetIsArmy1, targetIndex, targetName, flightMs);

        public void PlayHeal(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlayHeal(isArmy1, unitIndex, unitName);

        public void PlaySpell(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlaySpell(isArmy1, unitIndex, unitName);

        public void PlayCloneFlash(bool isArmy1, int unitIndex, string unitName) =>
            _animator.PlayCloneFlash(isArmy1, unitIndex, unitName);

        public void PlayCloneAppear(bool isArmy1, string cloneName) =>
            _animator.PlayCloneAppear(isArmy1, cloneName);

        public void PlayBuffAdded(bool isArmy1, int unitIndex, string unitName = "") =>
            _animator.PlayBuffAdded(isArmy1, unitIndex, unitName);

        // Лог событий 
        public void LogEvent(BattleEvent e) => _eventLogger.LogEvent(e);

        public void ClearLog() => _eventLogger.Clear();
    }
}