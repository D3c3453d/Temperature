using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Temperature.Framework.Data
{
    public static class DefaultConsts
    {

        public const float AbsoluteZero = -273;

        // ENV TEMP MODUL
        public const float EnvTemp = 25;

        public const float MaxSeasonTemp = 30;
        public const float MinSeasonTemp = -10;

        public const float DayCycleScale = 4;
        public const float FluctuationScale = 1;

        // MINES
        public const string MineName = "UndergroundMine";


        public const float MaxFrostMineTemp = 0;
        public const float MinFrostMineTemp = -15;

        public const float MaxUpperMineTemp = 30;
        public const float MinUpperMineTemp = 15;

        public const float LavaMineTempSlope = 1;
        public const float SkullCaveTempSlope = 0.01f;

        // DEVICES
        public const string HeatingType = "heating";
        public const string CoolingType = "cooling";
        public const string GeneralType = "general";


        // BODY TEMP MODUL
        public const float BodyTemp = 36;

        public const float MinComfyTemp = 16;

        public const float MaxComfyTemp = 26;

        public const float HypothermiaBodyTempThreshold = 35;

        public const float FrostbiteBodyTempThreshold = 30;

        public const float HeatstrokeBodyTempThreshold = 38.5f;

        public const float BurnBodyTempThreshold = 41;

        public const float LowTemperatureSlope = -0.17f;

        public const float HighTemperatureSlope = 0.09f;

        public const float TemperatureChangeEasing = 0.5f;
    }
}