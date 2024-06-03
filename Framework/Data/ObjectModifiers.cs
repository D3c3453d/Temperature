namespace Temperature.Framework.Data
{
    public class ObjectModifiers
    {
        public float CoreTemp { get; set; } = 0;
        //in tile
        public float EffectiveRange { get; set; } = 1;
        public bool NeedActive { get; set; } = false;
        //0 for normal detection (for furniture), 1 for machine detection (need power)
        public bool ActiveType { get; set; } = false;
        //ambient temp manipulation
        public float AmbientCoefficient { get; set; } = 1;

        public string DeviceType { get; set; } = "general";
        //operational range, only valid with general devices
        public float OperationalRange { get; set; } = 0;
    }
}