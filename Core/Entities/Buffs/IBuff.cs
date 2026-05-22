namespace Core.Entities.Buffs
{
    public interface IBuff
    {
        // Для отображения в имени юнита: "с Щитом", "с Копьем"
        string NameInstrumental { get; }

        // Для лога: "Щит", "Копьё"
        string NameNominative { get; }

        string BuffType { get; }
        int AttackBonus { get; }
        int DefenceBonus { get; }
    }
}