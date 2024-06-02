namespace Temperature
{
    public class Config
    {
        public float DefaultEnvTemp { get; set; } = 25;
        public float DefaultBodyTemp { get; set; } = 36;
        public float DefaultMinComfyTemp { get; set; } = 16;
        public float DefaultMaxComfyTemp { get; set; } = 26;
        public float HypothermiaBodyTempThreshold { get; set; } = 35;
        public float FrostbiteBodyTempThreshold { get; set; } = 30;
        public float HeatstrokeBodyTempThreshold { get; set; } = 38.5f;
        public float BurnBodyTempThreshold { get; set; } = 41;
        public float LowTemperatureSlope { get; set; } = -0.17f;
        public float HighTemperatureSlope { get; set; } = 0.09f;
        public float TemperatureChangeEasing { get; set; } = 0.5f;
        // MULTIPLIERS
        public float FarmIndoorTemperatureMultiplier { get; set; } = 0.5f;
        public float IndoorTemperatureMultiplier { get; set; } = 0.9f;
    }
}