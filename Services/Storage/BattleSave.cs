using System;
using System.Collections.Generic;

namespace Services.Storage
{
    public class BattleSave
    {
        public DateTime SavedAtUtc { get; set; }

        public string DisplayName { get; set; } = "";

        public int Turns { get; set; }
        public bool Army1Turn { get; set; }
        public int ScoreArmy1 { get; set; }
        public int ScoreArmy2 { get; set; }
        public string FormationType { get; set; } = "Bridge";

        public ArmySnapshot Army1 { get; set; } = new();
        public ArmySnapshot Army2 { get; set; } = new();

        public bool IsFinished { get; set; }
        public string Winner { get; set; } = "";

        public List<string> LogLines { get; set; } = new();
    }
}