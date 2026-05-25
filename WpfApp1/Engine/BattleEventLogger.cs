using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfPresentation.Engine;

namespace WpfPresentation.Views
{
    // Отвечает за отображение лога событий боя
    public class BattleEventLogger
    {
        private readonly StackPanel _logPanel;
        private readonly ScrollViewer _logScroller;

        public BattleEventLogger(StackPanel logPanel, ScrollViewer logScroller)
        {
            _logPanel = logPanel;
            _logScroller = logScroller;
        }

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
                    if (e.Damage > 0) { AddText(panel, $" → -{e.Damage} HP", "#E24B4A"); AddText(panel, $" ({e.HpBefore}→{e.HpAfter})", "#444441"); }
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
                    AddText(panel, " вручает ", "#5F5E5A");
                    AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    AddText(panel, $" — {e.Message}", "#EF9F27");
                    break;
                case BattleEventType.Spell:
                    AddText(panel, e.ActorName, e.ActorIsArmy1 ? "#B4B2A9" : "#C47A6A");
                    if (e.Message == "Клонирование")
                    { AddText(panel, " клонирует ", "#5F5E5A"); AddText(panel, e.TargetName, e.TargetIsArmy1 ? "#B4B2A9" : "#C47A6A"); }
                    else AddText(panel, " использует заклинание", "#7F77DD");
                    break;
                case BattleEventType.RoundEnd:
                    _logPanel.Children.Add(new TextBlock
                    {
                        Text = $"— РАУНД {e.Round} —",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x5F, 0x5E, 0x5A)),
                        Margin = new Thickness(0, 6, 0, 2)
                    });
                    _logScroller.ScrollToBottom();
                    return;
                default: return;
            }

            _logPanel.Children.Add(panel);
            _logScroller.ScrollToBottom();
        }

        public void Clear() => _logPanel.Children.Clear();

        private void AddText(StackPanel panel, string text, string hexColor)
        {
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            panel.Children.Add(new TextBlock
            {
                Text = text,
                FontFamily = new System.Windows.Media.FontFamily("Georgia"),
                FontSize = 14,
                Foreground = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Center
            });
        }
    }
}