namespace WpfPresentation.Engine
{
    public enum BattleEventType
    {
        MeleeHit,       // ближний удар
        MeleeMiss,      // удар нанёс 0 урона
        Death,          // юнит погиб
        ArrowShot,      // лучник выстрелил
        ArrowMiss,      // стрела не долетела
        Heal,           // лечение
        HealNoEffect,   // лечить не нужно
        Spell,          // заклинание мага
        BuffLost,       // потерян бафф
        BuffAdded,      // добавлен бафф
        RoundEnd,       // конец раунда
        BattleEnd,      // конец боя
    }

    public record BattleEvent
    {
        public BattleEventType Type { get; init; }

        // Кто совершил действие
        public bool ActorIsArmy1 { get; init; }
        public int ActorIndex { get; init; }
        public string ActorName { get; init; } = "";

        // На кого направлено действие
        public bool TargetIsArmy1 { get; init; }
        public int TargetIndex { get; init; }
        public string TargetName { get; init; } = "";

        // Доп. данные
        public int Damage { get; init; }
        public int HpBefore { get; init; }
        public int HpAfter { get; init; }
        public string Message { get; init; } = "";

        // Итог раунда
        public int Score1 { get; init; }
        public int Score2 { get; init; }
        public int Round { get; init; }

        // Итог боя
        public string? Winner { get; init; }
    }
}