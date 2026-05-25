using System.Windows;
using System.Windows.Input;
using Core.Interfaces;

namespace WpfPresentation.Views
{
    public partial class BattleResultWindow
    {
        public BattleResultWindow(
            Window owner,
            string winner,
            int score1, int score2,
            IArmy army1, IArmy army2,
            int rounds)
        {
            InitializeComponent();
            Owner = owner;

            WinnerText.Text = winner;

            Army1NameText.Text = army1.Name;
            Score1Text.Text = score1.ToString();
            int alive1 = army1.Units.Count(u => u.IsAlive);
            Alive1Text.Text = alive1 > 0 ? $"{alive1} выжили" : "все пали";

            Army2NameText.Text = army2.Name;
            Score2Text.Text = score2.ToString();
            int alive2 = army2.Units.Count(u => u.IsAlive);
            Alive2Text.Text = alive2 > 0 ? $"{alive2} выжили" : "все пали";

            RoundsText.Text = $"Сражение длилось {rounds} {RoundWord(rounds)}";

            // Победившая армия 
            if (score1 > score2)
                Score1Text.Foreground = System.Windows.Media.Brushes.Goldenrod;
            else if (score2 > score1)
                Score2Text.Foreground = System.Windows.Media.Brushes.Goldenrod;
        }

        private static string RoundWord(int n)
        {
            int mod10 = n % 10, mod100 = n % 100;
            if (mod100 >= 11 && mod100 <= 19) return "раундов";
            return mod10 switch { 1 => "раунд", 2 or 3 or 4 => "раунда", _ => "раундов" };
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}