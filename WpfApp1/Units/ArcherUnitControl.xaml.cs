using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfPresentation.Views.Units
{
    public partial class ArcherUnitControl : UserControl
    {
        public static readonly DependencyProperty IsArmy1Property =
            DependencyProperty.Register(nameof(IsArmy1), typeof(bool), typeof(ArcherUnitControl),
                new PropertyMetadata(true, OnIsArmy1Changed));

        public bool IsArmy1
        {
            get => (bool)GetValue(IsArmy1Property);
            set => SetValue(IsArmy1Property, value);
        }

        private static readonly Color Army1Color = Color.FromRgb(0xB4, 0xB2, 0xA9);
        private static readonly Color Army2Color = Color.FromRgb(0xC4, 0x7A, 0x6A);

        public ArcherUnitControl()
        {
            InitializeComponent();
        }

        private static void OnIsArmy1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcherUnitControl ctrl) ctrl.UpdateColor();
        }

        private void UpdateColor()
        {
            if (Resources["StrokeBrush"] is SolidColorBrush brush)
                brush.Color = IsArmy1 ? Army1Color : Army2Color;
        }

        public void PlayShoot()
        {
            var sb = (Storyboard)Resources["AttackAnim"];
            var moveAnim = (DoubleAnimation)sb.Children[0];
            moveAnim.To = IsArmy1 ? 30 : -30;
            sb.Begin();
        }

        public void PlayHit() => ((Storyboard)Resources["HitAnim"]).Begin(Root);

        public void PlayDeath(Action? onComplete = null)
        {
            var sb = (Storyboard)Resources["DeathAnim"];
            if (onComplete != null) sb.Completed += (_, _) => onComplete();
            sb.Begin();
        }
    }
}