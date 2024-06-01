namespace Temperature
{
    public class Config
    {
        // MULTIPLIERS
        public float FarmIndoorTemperatureMultiplier { get; set; } = 0.5f;

        public float IndoorTemperatureMultiplier { get; set; } = 0.9f;

        public float TempVelocity { get; set; } = 1f;
    }
}