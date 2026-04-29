using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfPresentation.Views
{
    public partial class BudgetView : UserControl
    {
        public event Action? BackRequested;
        public event Action<int>? BudgetConfirmed;

        public BudgetView()
        {
            InitializeComponent();
            BackButton.Click += (_, _) => BackRequested?.Invoke();
            NextButton.Click += (_, _) => OnNext();
            BudgetInput.Focus();
        }

        private void OnNext()
        {
            if (!int.TryParse(BudgetInput.Text, out int budget) || budget <= 0)
            {
                ErrorText.Text = "Введите корректное число больше нуля.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;
            BudgetConfirmed?.Invoke(budget);
        }

        // Разрешаем вводить только цифры
        private void BudgetInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}