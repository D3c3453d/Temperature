namespace Temperature.Framework.Data
{
    public class ClothingModifiers
    {
        public string Pattern { get; set; } = "Equals"; // Equals StartsWith EndsWith Contains
        public float HeatResistance { get; set; } = 0;
        public float ColdResistance { get; set; } = 0;
    }
}
