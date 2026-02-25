namespace gaaameee.Core.Entities
{
    // Результат завершённого боя.
    public class BattleResult
    {
        // Победившая армия
        public string Winner { get; }

        // Количество ходов в бою
        public int Turns { get; }

        // Результат боя
        public BattleResult(string winner, int turns)
        {
            Winner = winner;
            Turns = turns;
        }

        public override string ToString()
        {
            return $"Winner: {Winner}, Turns: {Turns}";
        }
    }
}