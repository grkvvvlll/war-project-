namespace Services.Storage
{
    public class ArmySnapshot
    {
        public string Name { get; set; } = "";
        public List<UnitSnapshot> Units { get; set; } = new();
    }
}