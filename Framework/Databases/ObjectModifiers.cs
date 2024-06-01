namespace Temperature.Framework.Databases
{
    public class ObjectModifiers
    {
        public float coreTemp { get; set; } = 0;
        //in tile
        public float effectiveRange { get; set; } = 1;
        public bool needActive { get; set; } = false;
        //0 for normal detection (for furniture), 1 for machine detection (need power)
        public int activeType { get; set; } = 0;
        //ambient temp manipulation
        public float ambientCoefficient { get; set; } = 1;

        public string deviceType { get; set; } = "general";
        //operational range, only valid with general devices
        public float operationalRange { get; set; } = 0;
    }
}