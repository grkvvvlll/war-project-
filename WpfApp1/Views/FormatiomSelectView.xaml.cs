using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.Formations;
using Core.Interfaces;

namespace WpfPresentation.Views
{
    public partial class FormationSelectView : UserControl
    {
        public event Action? BackRequested;
        public event Action<IBattleFormation>? FormationSelected;

        private int _selectedCard = 1;

        private static readonly SolidColorBrush BorderActive = new(Color.FromRgb(0x88, 0x87, 0x80));
        private static readonly SolidColorBrush BorderMuted = new(Color.FromRgb(0x5F, 0x5E, 0x5A));
        private static readonly SolidColorBrush BgSelected = new(Color.FromArgb(0x1A, 0xFA, 0xC7, 0x75));
        private static readonly SolidColorBrush BgTransparent = Brushes.Transparent;
        private static readonly SolidColorBrush GoldBrush = new(Color.FromRgb(0xFA, 0xC7, 0x75));
        private static readonly SolidColorBrush TextPrimary = new(Color.FromRgb(0xD3, 0xD1, 0xC7));

        public FormationSelectView()
        {
            InitializeComponent();
            BackButton.Click += (_, _) => BackRequested?.Invoke();
            NextButton.Click += (_, _) => OnNext();
            SelectCard(1);
        }

        private void Card1_Click(object sender, MouseButtonEventArgs e) => SelectCard(1);
        private void Card2_Click(object sender, MouseButtonEventArgs e) => SelectCard(2);
        private void Card3_Click(object sender, MouseButtonEventArgs e) => SelectCard(3);

        private void SelectCard(int n)
        {
            _selectedCard = n;

            SetCardStyle(Card1, false);
            SetCardStyle(Card2, false);
            SetCardStyle(Card3, false);

            var selected = n switch { 1 => Card1, 2 => Card2, _ => Card3 };
            SetCardStyle(selected, true);
        }

        private void SetCardStyle(Border card, bool selected)
        {
            card.BorderBrush = selected ? BorderActive : BorderMuted;
            card.Background = selected ? BgSelected : BgTransparent;

            if (card.Child is StackPanel sp && sp.Children.Count > 1)
            {
                var icon = sp.Children[0] as TextBlock;
                var title = sp.Children[1] as TextBlock;
                if (icon != null) icon.Foreground = selected ? GoldBrush : BorderMuted;
                if (title != null) title.Foreground = selected ? GoldBrush : TextPrimary;
            }
        }

        private void OnNext()
        {
            IBattleFormation formation = _selectedCard switch
            {
                1 => new BridgeFormation(),
                2 => new WideBridgeFormation(),
                _ => new WallFormation()
            };
            FormationSelected?.Invoke(formation);
        }
    }
}