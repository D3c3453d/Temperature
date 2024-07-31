using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Temperature.Framework.Data
{
    public static class DefaultConsts
    {

        public const float AbsoluteZero = -273;

        // ENV TEMP MODUL
        public const float EnvTemp = 25;

        public const float MaxSeasonTemp = 27;
        public const float MinSeasonTemp = -10;

        public const float DayCycleScale = 4;
        public const float DayCycleOffset = 9;
        public const float DayCycleStretch = 12;

        public const float FluctuationScale = 1;

        public const string FullSeasonName = "Full";

        // MINES
        public const string MineName = "UndergroundMine";


        public const float MaxFrostMineTemp = 0;
        public const float MinFrostMineTemp = -20;

        public const float MaxUpperMineTemp = 30;
        public const float MinUpperMineTemp = 15;

        public const float LavaMineTempSlope = 1;
        public const float SkullCaveTempSlope = 0.01f;

        // DEVICES
        public const string HeatingType = "heating";
        public const string CoolingType = "cooling";
        public const string GeneralType = "general";


        // BODY TEMP MODUL
        public const float BodyTemp = 0;
        public const float MinBodyTemp = -1;
        public const float MaxBodyTemp = 1;
        public const float BodyTempSlope = 0.1f;
        public const float MinComfyTemp = 16;
        public const float MaxComfyTemp = 28;
        public const float MinStep = 0.00001f;
    }
}