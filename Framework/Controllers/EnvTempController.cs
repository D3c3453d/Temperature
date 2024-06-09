using System;
using System.Collections.Generic;
using StardewValley;
using Temperature.Framework.Data;
using Temperature.Framework.Misc;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace Temperature.Framework.Controllers
{
    public static class EnvTempController
    {
        private static float fluctuation = 0;
        private static float dayCycleScale = 0;
        private static float fluctuationScale = 0;
        private static readonly Random rand = new();


        private static void ApplyMines(ref float envTemp, int currentMineLevel)
        {
            switch (currentMineLevel)
            {
                case MineShaft.quarryMineShaft:
                    envTemp = DefaultConsts.EnvTemp;
                    break;
                case >= MineShaft.bottomOfMineLevel:
                    envTemp =
                    CalcHelper.StraightThroughPoint(currentMineLevel, DefaultConsts.SkullCaveTempSlope, MineShaft.bottomOfMineLevel, DefaultConsts.EnvTemp);
                    break;
                case >= MineShaft.mineLavaLevel:
                    envTemp =
                    CalcHelper.StraightThroughPoint(currentMineLevel, DefaultConsts.LavaMineTempSlope, MineShaft.mineLavaLevel, DefaultConsts.EnvTemp);
                    break;
                case >= MineShaft.mineFrostLevel:
                    envTemp =
                    CalcHelper.ParabolaWithCentralExtremum(currentMineLevel, DefaultConsts.MinFrostMineTemp, DefaultConsts.MaxFrostMineTemp, MineShaft.mineFrostLevel, MineShaft.mineLavaLevel);
                    break;
                case > MineShaft.upperArea:
                    envTemp =
                    CalcHelper.ParabolaWithCentralExtremum(currentMineLevel, DefaultConsts.MaxUpperMineTemp, DefaultConsts.MinUpperMineTemp, MineShaft.upperArea, MineShaft.mineFrostLevel);
                    break;
            }
        }

        private static void ApplySeasonCycleTemp(ref float envTemp, EnvModifiers envData, int totalDays)
        {
            switch (envData.MinValue, envData.MaxValue)
            {
                case ( > DefaultConsts.AbsoluteZero, > DefaultConsts.AbsoluteZero):
                    envTemp = CalcHelper.SinWithMinMax(totalDays, envData.MinValue, envData.MaxValue, WorldDate.DaysPerMonth / 2, WorldDate.DaysPerMonth * 2);
                    break;
                case ( > DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero):
                    envTemp = CalcHelper.SinWithMinMax(totalDays, envData.MinValue, envData.MinValue, WorldDate.DaysPerMonth / 2, WorldDate.DaysPerMonth * 2);
                    break;
                case (DefaultConsts.AbsoluteZero, > DefaultConsts.AbsoluteZero):
                    envTemp = CalcHelper.SinWithMinMax(totalDays, envData.MaxValue, envData.MaxValue, WorldDate.DaysPerMonth / 2, WorldDate.DaysPerMonth * 2);
                    break;
            }
        }

        private static void ApplyEnvData(ref float envTemp, EnvModifiers envData, int totalDays)
        {
            // season or weather or location temperature modifiers

            ApplySeasonCycleTemp(ref envTemp, envData, totalDays);
            envTemp *= envData.Multiplicative;
            envTemp += envData.Additive;
            dayCycleScale += envData.DayCycleScale;
            fluctuationScale += envData.FluctuationScale;
        }

        private static void ApplyLocation(ref float envTemp, EnvModifiers envData, GameLocation location, int totalDays, int currentMineLevel)
        {
            // default location temperature modifiers
            if (location.Name == DefaultConsts.MineName + currentMineLevel)
            {
                ApplyMines(ref envTemp, currentMineLevel);
                envData.DayCycleScale = 0; // there is no day cycle income in mines
            }
            else if (!location.IsOutdoors)
            {
                float indoorModifier;
                if (location.IsFarm)
                    indoorModifier = (DefaultConsts.EnvTemp - envTemp) * ModEntry.Config.FarmIndoorTemperatureMultiplier;
                else
                    indoorModifier = (DefaultConsts.EnvTemp - envTemp) * ModEntry.Config.IndoorTemperatureMultiplier;
                envTemp += indoorModifier;
            }

            // custom locatin temperature modifiers

            ApplyEnvData(ref envTemp, envData, totalDays);
        }

        private static void ApplyObjects(ref float envTemp, GameLocation location, float playerTileX, float playerTileY)
        {
            // local temperature emitted by objects

            float proximityCheckBound = DataController.ObjectsMaxEffectiveRange;
            List<SObject> nearbyObjects = [];
            for (float i = playerTileX - proximityCheckBound; i <= playerTileX + proximityCheckBound; i++)
                for (float j = playerTileY - proximityCheckBound; j <= playerTileY + proximityCheckBound; j++)
                {
                    SObject obj = location.getObjectAtTile((int)i, (int)j);

                    if (obj != null && !nearbyObjects.Contains(obj)) // skip objects, that already calculated
                    {
                        ObjectModifiers objectData = DataController.Objects.Data.GetValueOrDefault(obj.Name);
                        if (objectData != null)
                        {
                            nearbyObjects.Add(obj);

                            // skip inactive objects, that need to be activated
                            if (objectData.NeedActive && !CalcHelper.CheckIfItemIsActive(obj, objectData.ActiveType)) continue;

                            // prioritize ambient temp if it exceed device's core temp
                            if ((objectData.DeviceType == DefaultConsts.HeatingType && objectData.CoreTemp < envTemp) ||
                                (objectData.DeviceType == DefaultConsts.CoolingType && objectData.CoreTemp > envTemp))
                                continue;

                            // dealing with env temp
                            float dist = MathF.Max(CalcHelper.Distance(CalcHelper.PixelToTile(obj.GetBoundingBox().Center.X), CalcHelper.PixelToTile(obj.GetBoundingBox().Center.Y), playerTileX, playerTileY), 1);
                            if (dist <= objectData.EffectiveRange)
                            {
                                float tempModifierEntry = (objectData.CoreTemp - envTemp) / (15 * (dist - 1) / objectData.EffectiveRange + 1);
                                envTemp += tempModifierEntry;
                            }
                        }
                    }
                }
        }

        private static void ApplyAmbient(ref float envTemp, GameLocation location)
        {
            // ambient by temperature control objects

            if (!location.IsOutdoors)
            {
                var objectsData = new List<ObjectModifiers>();

                // objects
                foreach (SObject obj in location.objects.Values)
                {
                    ObjectModifiers objectData = DataController.Objects.Data.GetValueOrDefault(obj.Name);
                    if (objectData != null)
                        if (!objectData.NeedActive || CalcHelper.CheckIfItemIsActive(obj, objectData.ActiveType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                // furniture
                foreach (SObject obj in location.furniture)
                {
                    ObjectModifiers objectData = DataController.Objects.Data.GetValueOrDefault(obj.Name);
                    if (objectData != null)
                        if (!objectData.NeedActive || CalcHelper.CheckIfItemIsActive(obj, objectData.ActiveType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                float area = location.Map.GetLayer("Back").Tiles.Array.Length;
                float power = 0;

                foreach (ObjectModifiers objectData in objectsData)
                {
                    //calculate indoor heating power base on core temp and range
                    if (objectData.DeviceType == DefaultConsts.GeneralType)
                    {
                        float perfectAmbientPower = area * DefaultConsts.EnvTemp;
                        float maxPowerFromDevice = objectData.OperationalRange * (objectData.EffectiveRange * 2 + 1) * (objectData.EffectiveRange * 2 + 1) * objectData.AmbientCoefficient;
                        if (DefaultConsts.EnvTemp > envTemp)
                            power = MathF.Min(perfectAmbientPower, power + maxPowerFromDevice);
                        else
                            power = MathF.Max(perfectAmbientPower, power - maxPowerFromDevice);
                    }
                    else power += (objectData.CoreTemp - DefaultConsts.EnvTemp) * (objectData.EffectiveRange * 2 + 1) * (objectData.EffectiveRange * 2 + 1) * objectData.AmbientCoefficient;
                }
                envTemp += 0.5f * power / area;
            }
        }

        private static void ApplyDayCycle(ref float envTemp, int timeOfDay)
        {
            float hours = timeOfDay / 100 + timeOfDay % 100 / 60.0f; // converting ingame "digital clock" time to irl hours with floating point
            envTemp += CalcHelper.SinWithMinMax(hours, -dayCycleScale, dayCycleScale, DefaultConsts.DayCycleOffset, DefaultConsts.DayCycleStretch);
        }

        public static float Update(EnvModifiers seasonData,
        EnvModifiers weatherData, EnvModifiers locationData, GameLocation location,
        int playerAbsoluteX, int playerAbsoluteY, int totalDays, int currentMineLevel)
        {
            // main func
            // OnSecondPassed
            float envTemp = 0;
            dayCycleScale = 0;
            fluctuationScale = 0;
            LogHelper.Alert($"seasonData {seasonData.MinValue} {seasonData.MaxValue} {seasonData.DayCycleScale} {seasonData.FluctuationScale} {seasonData.Additive} {seasonData.Multiplicative}");
            LogHelper.Alert($"weatherData {weatherData.MinValue} {weatherData.MaxValue} {weatherData.DayCycleScale} {weatherData.FluctuationScale} {weatherData.Additive} {weatherData.Multiplicative}");
            LogHelper.Alert($"locationData {locationData.MinValue} {locationData.MaxValue} {locationData.DayCycleScale} {locationData.FluctuationScale} {locationData.Additive} {locationData.Multiplicative}");

            float oldTemp = envTemp;
            if (location != null)
            {
                ApplyEnvData(ref envTemp, seasonData, totalDays); // apply season
                LogHelper.Error($"season income {envTemp - oldTemp}");
                oldTemp = envTemp;
                ApplyEnvData(ref envTemp, weatherData, totalDays); // apply weather
                LogHelper.Error($"weather income {envTemp - oldTemp}");
                oldTemp = envTemp;
                ApplyLocation(ref envTemp, locationData, location, totalDays, currentMineLevel); // apply location
                LogHelper.Error($"loc income {envTemp - oldTemp}");
                oldTemp = envTemp;

                ApplyObjects(ref envTemp, location, CalcHelper.PixelToTile(playerAbsoluteX), CalcHelper.PixelToTile(playerAbsoluteY));
                LogHelper.Error($"objects income {envTemp - oldTemp}");
                oldTemp = envTemp;
                ApplyAmbient(ref envTemp, location);
                LogHelper.Error($"ambient income {envTemp - oldTemp}");
                oldTemp = envTemp;
            }
            else
            {
                envTemp = DefaultConsts.EnvTemp;
                fluctuationScale = 0;
                dayCycleScale = 0;
                LogHelper.Error($"null income {envTemp - oldTemp}");
                oldTemp = envTemp;
            }

            ApplyDayCycle(ref envTemp, Game1.timeOfDay);
            LogHelper.Error($"day cycle income {envTemp - oldTemp}");
            oldTemp = envTemp;
            envTemp += fluctuation;
            LogHelper.Error($"fluc income {envTemp - oldTemp}");

            return envTemp;
        }

        public static void FluctuationUpdate()
        {
            // fluctuation
            // OnTimeChanged
            fluctuation = (float)rand.NextDouble() * fluctuationScale - 0.5f * fluctuationScale;
        }
    }
}