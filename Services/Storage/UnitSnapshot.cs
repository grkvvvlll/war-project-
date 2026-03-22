namespace Services.Storage
{
    public class UnitSnapshot
    {
        public string UnitType { get; set; } = "";
        public string Name { get; set; } = "";

        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Cost { get; set; }

        public int? Range { get; set; }
        public int? HealRange { get; set; }
        public int? HealPower { get; set; }
        public int? SpellRange { get; set; }
        public int? ClonePower { get; set; }
    }
}