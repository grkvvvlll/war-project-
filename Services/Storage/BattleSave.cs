using System;
using System.Collections.Generic;

namespace Services.Storage
{
    public class BattleSave
    {
        public DateTime SavedAtUtc { get; set; }
        public string Winner { get; set; } = "";
        public int Turns { get; set; }
        public List<string> LogLines { get; set; } = new();
    }
}