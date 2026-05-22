using System.Windows;
using System.Windows.Input;

namespace WpfPresentation.Views
{
    public partial class SaveGameWindow
    {
        public string SaveName { get; private set; } = "";

        public SaveGameWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            Loaded += (_, _) =>
            {
                NameBox.Focus();
                NameBox.SelectAll();
            };
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void NameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
                PlaceholderText.Visibility = string.IsNullOrEmpty(NameBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TrySave();
            else if (e.Key == Key.Escape)
                DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => TrySave();

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void TrySave()
        {
            SaveName = NameBox.Text.Trim();
            DialogResult = true;
        }
    }
}