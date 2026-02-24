namespace gaaameee.Core.Entities
{
    public class BattleResult
    {
        public string Winner { get; }
        public int Turns { get; }

        public BattleResult(string winner, int turns)
        {
            Winner = winner;
            Turns = turns;
        }
    }
}