namespace Core.Interfaces
{
    /// <summary>
    /// Абстракция UI-слоя для боя
    /// </summary>
    public interface IBattleUI
    {
        RoundMenuChoice WaitForChoice(int roundNumber);
        void PrintArmyState(IArmy army1, IArmy army2);
        void PrintHistory(IEnumerable<string> entries);
        void PrintSaved(string fileName);
        void PrintSaveFailed();
        void PrintMessage(string message);
        string ReadSaveName();
        IBattleFormation? ReadFormationChoice();
    }
}