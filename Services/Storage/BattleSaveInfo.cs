using System;

namespace Services.Storage
{
    public class BattleSaveInfo
    {
        public string FileName { get; set; } = "";
        public DateTime SavedAtUtc { get; set; }
        public string Winner { get; set; } = "";
        public int Turns { get; set; }
    }
}