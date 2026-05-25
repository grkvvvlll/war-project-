using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfPresentation.Views.Units;

namespace WpfPresentation.Views
{
    // Отвечает за анимации событий боя
    public class BattleAnimator
    {
        private readonly System.Windows.Controls.Canvas _canvas;
        private readonly Func<bool, int, string, UserControl?> _findControl;
        private readonly Dictionary<(bool isArmy1, string name), UserControl> _byName;
        private readonly double _unitWidth;
        private readonly double _unitHeight;

        public BattleAnimator(
            System.Windows.Controls.Canvas canvas,
            Func<bool, int, string, UserControl?> findControl,
            Dictionary<(bool isArmy1, string name), UserControl> byName,
            double unitWidth,
            double unitHeight)
        {
            _canvas = canvas;
            _findControl = findControl;
            _byName = byName;
            _unitWidth = unitWidth;
            _unitHeight = unitHeight;
        }

        public void PlayAttack(bool isArmy1, int unitIndex, string unitName = "")
        {
            var ctrl = _findControl(isArmy1, unitIndex, unitName);
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
            var ctrl = _findControl(isArmy1, unitIndex, unitName);
            if (ctrl != null) ShowOverlay(ctrl, Color.FromArgb(180, 220, 50, 50), 480);
        }

        public void PlayHealTarget(bool isArmy1, int unitIndex, string unitName = "")
        {
            var ctrl = _findControl(isArmy1, unitIndex, unitName);
            if (ctrl != null) ShowOverlay(ctrl, Color.FromArgb(160, 30, 200, 100), 600);
        }

        public void PlayDeath(bool isArmy1, int unitIndex, string unitName = "", Action? onComplete = null)
        {
            var ctrl = _findControl(isArmy1, unitIndex, unitName);
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
            if (_findControl(isArmy1, unitIndex, unitName) is ArcherUnitControl a) a.PlayShoot();
        }

        public void PlayArrow(bool archerIsArmy1, int archerIndex, string archerName,
            bool targetIsArmy1, int targetIndex, string targetName, int flightMs = 500)
        {
            var archerCtrl = _findControl(archerIsArmy1, archerIndex, archerName);
            var targetCtrl = _findControl(targetIsArmy1, targetIndex, targetName);
            if (archerCtrl == null || targetCtrl == null) return;

            double ax = System.Windows.Controls.Canvas.GetLeft(archerCtrl) + (archerIsArmy1 ? _unitWidth : 0);
            double ay = System.Windows.Controls.Canvas.GetTop(archerCtrl) + _unitHeight * 0.38;
            double tx = System.Windows.Controls.Canvas.GetLeft(targetCtrl) + _unitWidth / 2;
            double ty = System.Windows.Controls.Canvas.GetTop(targetCtrl) + _unitHeight * 0.38;
            double dx = tx - ax, dy = ty - ay;
            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            var arrow = new Rectangle { Width = 18, Height = 3, Fill = Brush(0xC8, 0xA0, 0x50), IsHitTestVisible = false, RenderTransformOrigin = new Point(0, 0.5) };
            var rotate = new RotateTransform(angle);
            var translate = new TranslateTransform();
            var group = new TransformGroup();
            group.Children.Add(rotate); group.Children.Add(translate);
            arrow.RenderTransform = group;
            System.Windows.Controls.Canvas.SetLeft(arrow, ax);
            System.Windows.Controls.Canvas.SetTop(arrow, ay);
            _canvas.Children.Add(arrow);

            var ease = new QuadraticEase { EasingMode = EasingMode.EaseIn };
            var animX = new DoubleAnimation { To = dx, Duration = TimeSpan.FromMilliseconds(flightMs), EasingFunction = ease };
            var animY = new DoubleAnimation { To = dy, Duration = TimeSpan.FromMilliseconds(flightMs), EasingFunction = ease };
            animY.Completed += (_, _) => _canvas.Children.Remove(arrow);
            translate.BeginAnimation(TranslateTransform.XProperty, animX);
            translate.BeginAnimation(TranslateTransform.YProperty, animY);
        }

        public void PlayHeal(bool isArmy1, int unitIndex, string unitName = "")
        {
            if (_findControl(isArmy1, unitIndex, unitName) is HealerUnitControl h) h.PlayHeal();
        }

        public void PlaySpell(bool isArmy1, int unitIndex, string unitName = "")
        {
            if (_findControl(isArmy1, unitIndex, unitName) is WizardUnitControl w) w.PlaySpell();
        }

        public void PlayCloneFlash(bool isArmy1, int unitIndex, string unitName)
        {
            var ctrl = _findControl(isArmy1, unitIndex, unitName);
            if (ctrl == null) return;
            double cx = System.Windows.Controls.Canvas.GetLeft(ctrl) + _unitWidth / 2;
            double cy = System.Windows.Controls.Canvas.GetTop(ctrl) + _unitHeight / 2;
            var flash = new Ellipse
            {
                Width = 8,
                Height = 8,
                IsHitTestVisible = false,
                Fill = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };
            System.Windows.Controls.Canvas.SetLeft(flash, cx - 4);
            System.Windows.Controls.Canvas.SetTop(flash, cy - 4);
            _canvas.Children.Add(flash);
            var sb = new Storyboard();
            foreach (var prop in new[] { "ScaleTransform.ScaleX", "ScaleTransform.ScaleY" })
            {
                var a = new DoubleAnimation { From = 1, To = 14, Duration = TimeSpan.FromMilliseconds(550), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(a, flash);
                Storyboard.SetTargetProperty(a, new PropertyPath($"(UIElement.RenderTransform).({prop})"));
                sb.Children.Add(a);
            }
            var fadeOut = new DoubleAnimation { From = 0.9, To = 0, Duration = TimeSpan.FromMilliseconds(550), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            fadeOut.Completed += (_, _) => _canvas.Children.Remove(flash);
            Storyboard.SetTarget(fadeOut, flash);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(fadeOut);
            sb.Begin();
        }

        public void PlayCloneAppear(bool isArmy1, string cloneName)
        {
            if (!_byName.TryGetValue((isArmy1, cloneName), out var ctrl)) return;
            ctrl.Opacity = 0;
            var sb = new Storyboard();
            var fade = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(450), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(fade, ctrl);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(fade);
            sb.Begin();
        }

        public void PlayBuffAdded(bool isArmy1, int unitIndex, string unitName = "")
        {
            var ctrl = _findControl(isArmy1, unitIndex, unitName);
            if (ctrl != null) ShowOverlay(ctrl, Color.FromArgb(160, 239, 159, 39), 700);
        }

        private void ShowOverlay(UserControl ctrl, Color color, int durationMs)
        {
            var overlay = new Rectangle
            {
                Width = ctrl.Width,
                Height = ctrl.Height,
                Fill = new SolidColorBrush(color),
                IsHitTestVisible = false
            };
            System.Windows.Controls.Canvas.SetLeft(overlay, System.Windows.Controls.Canvas.GetLeft(ctrl));
            System.Windows.Controls.Canvas.SetTop(overlay, System.Windows.Controls.Canvas.GetTop(ctrl));
            _canvas.Children.Add(overlay);
            var sb = new Storyboard();
            var fade = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(durationMs) };
            fade.Completed += (_, _) => _canvas.Children.Remove(overlay);
            Storyboard.SetTarget(fade, overlay);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(fade);
            sb.Begin();
        }

        private static SolidColorBrush Brush(byte r, byte g, byte b) =>
            new(Color.FromRgb(r, g, b));
    }
}