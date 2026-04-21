using Core.Interfaces;

namespace Services.Storage
{
    public class BattleResumeData
    {
        public IArmy Army1 { get; set; } = null!;
        public IArmy Army2 { get; set; } = null!;

        public int Turns { get; set; }
        public bool Army1Turn { get; set; }
        public int ScoreArmy1 { get; set; }
        public int ScoreArmy2 { get; set; }
        public IBattleFormation Formation { get; set; } = null!;

        public bool IsFinished { get; set; }
        public string Winner { get; set; } = "";
    }
}