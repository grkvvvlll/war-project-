using System.Windows;
using System.Windows.Input;

namespace WpfPresentation.Views
{
    public partial class HelpWindow
    {
        public HelpWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Fade-out перед закрытием
            var anim = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = System.TimeSpan.FromMilliseconds(200),
            };
            anim.Completed += (_, _) => Close();
            RootBorder.BeginAnimation(OpacityProperty, anim);
        }
    }
}