namespace Temperature.Framework.Data
{
    public class ObjectModifiers
    {
        public float CoreTemp { get; set; } = 0; // temperature in tile
        public float EffectiveRange { get; set; } = 1;
        public bool NeedActive { get; set; } = false;
        public bool ActiveType { get; set; } = false; // true if the object is a machine for crafting (eg. Furnace, Charcoal Kiln)
        public float AmbientCoefficient { get; set; } = 1;
        public string DeviceType { get; set; } = "general";
        public float OperationalRange { get; set; } = 0;
    }
}
