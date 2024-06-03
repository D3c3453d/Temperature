using System.Collections.Generic;

namespace Temperature
{
    public class Config
    {
        // ENV TEMP MODUL

        public float FarmIndoorTemperatureMultiplier { get; set; } = 0.5f;

        public float IndoorTemperatureMultiplier { get; set; } = 0.9f;
    }
}