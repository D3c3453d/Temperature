using System;
using System.Collections.Generic;
using StardewValley;
using Temperature.Framework.Data;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace Temperature.Framework.Controllers
{
    public static class EnvTempController
    {
        private static float fluctuation = 0;
        private static float dayCycleScale = DefaultConsts.DayCycleScale;
        private static float fluctuationScale = DefaultConsts.FluctuationScale;
        private static readonly Random rand = new();

        private static float PixelToTile(int pixel)
        {
            return (float)pixel / Game1.tileSize;
        }

        private static float Distance(float aX, float aY, float bX, float bY)
        {
            return MathF.Sqrt((aX - bX) * (aX - bX) + (aY - bY) * (aY - bY));
        }

        private static float ParabolaWithCentralExtremum(float x, float extremumValue, float boundaryValue, float boundary1, float boundary2)
        {
            return 4 * (boundaryValue - extremumValue) /
            ((boundary1 - boundary2) * (boundary1 - boundary2)) *
            (x - (boundary1 + boundary2) / 2) * (x - (boundary1 + boundary2) / 2) + extremumValue;
        }

        private static float StraightThroughPoint(float x, float slope, float pointX, float pointY)
        {
            return slope * (x - pointX) + pointY;
        }

        private static bool CheckIfItemIsActive(SObject obj, bool activeType = false)
        {
            //check if the object is a machine for crafting (eg. Furnace, Charcoal Kiln)
            if (activeType)
            {
                if (obj.MinutesUntilReady > 0 && obj.heldObject.Value != null) return true;
                else return false;
            }
            else
            {
                //if not a machine for crafting (assuming furniture), check if said furniture is active
                if (obj.IsOn) return true;
                else return false;
            }
        }

        private static float SeasonCycleTemp(float minTemp, float maxTemp, int totalDays)
        {
            return (maxTemp - minTemp) / 2 * MathF.Sin((totalDays - WorldDate.DaysPerMonth / 2) * MathF.PI / (2 * WorldDate.DaysPerMonth)) + (maxTemp + minTemp) / 2;
        }

        private static void ApplySeasonCycleTemp(ref float envTemp, EnvModifiers envData, int totalDays)
        {
            switch (envData.MinValue, envData.MaxValue)
            {
                case ( > DefaultConsts.AbsoluteZero, > DefaultConsts.AbsoluteZero):
                    envTemp = SeasonCycleTemp(envData.MinValue, envData.MaxValue, totalDays);
                    break;
                case ( > DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero):
                    envTemp = SeasonCycleTemp(envData.MinValue, envData.MinValue, totalDays);
                    break;
                case (DefaultConsts.AbsoluteZero, > DefaultConsts.AbsoluteZero):
                    envTemp = SeasonCycleTemp(envData.MaxValue, envData.MaxValue, totalDays);
                    break;
            }
        }

        private static void ApplySeason(ref float envTemp, GameLocation location, int totalDays, string farmSeason)
        {
            // season temperature modifiers
            string season = location.GetSeason().ToString();
            if (season == farmSeason) season = DefaultConsts.FullSeasonName;

            EnvModifiers envData = DataController.Seasons.Data.GetValueOrDefault(season) ??
                new EnvModifiers(DefaultConsts.MaxSeasonTemp, DefaultConsts.MinSeasonTemp);

            ApplySeasonCycleTemp(ref envTemp, envData, totalDays);
            envTemp += envData.Additive;
            envTemp *= envData.Multiplicative;
            dayCycleScale += envData.DayCycleScale;
            fluctuationScale += envData.FluctuationScale;
        }

        private static void ApplyWeather(ref float envTemp, GameLocation location, int totalDays)
        {
            // weather temperature modifiers

            string weather = location.GetWeather().Weather;
            EnvModifiers envData = DataController.Weather.Data.GetValueOrDefault(weather) ?? new EnvModifiers();

            ApplySeasonCycleTemp(ref envTemp, envData, totalDays);
            envTemp += envData.Additive;
            envTemp *= envData.Multiplicative;
            dayCycleScale += envData.DayCycleScale;
            fluctuationScale += envData.FluctuationScale;
        }

        private static void ApplyMines(ref float envTemp, ref EnvModifiers envData, int currentMineLevel)
        {
            switch (currentMineLevel)
            {
                case MineShaft.quarryMineShaft:
                    envTemp = DefaultConsts.EnvTemp;
                    break;
                case >= MineShaft.bottomOfMineLevel:
                    envTemp =
                    StraightThroughPoint(currentMineLevel, DefaultConsts.SkullCaveTempSlope, MineShaft.bottomOfMineLevel, DefaultConsts.EnvTemp);
                    break;
                case >= MineShaft.mineLavaLevel:
                    envTemp =
                    StraightThroughPoint(currentMineLevel, DefaultConsts.LavaMineTempSlope, MineShaft.mineLavaLevel, DefaultConsts.EnvTemp);
                    break;
                case >= MineShaft.mineFrostLevel:
                    envTemp =
                    ParabolaWithCentralExtremum(currentMineLevel, DefaultConsts.MinFrostMineTemp, DefaultConsts.MaxFrostMineTemp, MineShaft.mineFrostLevel, MineShaft.mineLavaLevel);
                    break;
                case > MineShaft.upperArea:
                    envTemp =
                    ParabolaWithCentralExtremum(currentMineLevel, DefaultConsts.MaxUpperMineTemp, DefaultConsts.MinUpperMineTemp, MineShaft.upperArea, MineShaft.mineFrostLevel);
                    break;
            }
            envData.DayCycleScale = 0;
        }

        private static void ApplyLocation(ref float envTemp, GameLocation location, int totalDays, int currentMineLevel)
        {
            var envData = DataController.Locations.Data.GetValueOrDefault(location.Name) ??
                new EnvModifiers(DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero, DefaultConsts.DayCycleScale, DefaultConsts.FluctuationScale);

            // default location temperature modifiers
            if (location.Name == DefaultConsts.MineName + currentMineLevel)
                ApplyMines(ref envTemp, ref envData, currentMineLevel);
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
            ApplySeasonCycleTemp(ref envTemp, envData, totalDays);
            envTemp += envData.Additive;
            envTemp *= envData.Multiplicative;
            dayCycleScale += envData.DayCycleScale;
            fluctuationScale += envData.FluctuationScale;
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
                            if (objectData.NeedActive && !CheckIfItemIsActive(obj, objectData.ActiveType)) continue;

                            // prioritize ambient temp if it exceed device's core temp
                            if ((objectData.DeviceType == DefaultConsts.HeatingType && objectData.CoreTemp < envTemp) ||
                                (objectData.DeviceType == DefaultConsts.CoolingType && objectData.CoreTemp > envTemp))
                                continue;

                            // dealing with target temp this.value here?
                            float dist = MathF.Max(Distance(PixelToTile(obj.GetBoundingBox().Center.X), PixelToTile(obj.GetBoundingBox().Center.Y), playerTileX, playerTileY), 1);
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
                        if (!objectData.NeedActive || CheckIfItemIsActive(obj, objectData.ActiveType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                // furniture
                foreach (SObject obj in location.furniture)
                {
                    ObjectModifiers objectData = DataController.Objects.Data.GetValueOrDefault(obj.Name);
                    if (objectData != null)
                        if (!objectData.NeedActive || CheckIfItemIsActive(obj, objectData.ActiveType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                float area = location.Map.GetLayer("Back").Tiles.Array.Length;
                float power = 0;

                foreach (ObjectModifiers objectData in objectsData)
                {
                    //calculate indoor heating power base on core temp and range (assume full effectiveness if object is placed indoor)
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
            envTemp += MathF.Sin((hours - DefaultConsts.DayCycleOffset) * MathF.PI / DefaultConsts.DayCycleStretch) * dayCycleScale;
        }

        public static float Update()
        {
            // main func
            // OnSecondPassed
            float envTemp = 0;
            dayCycleScale = 0;
            fluctuationScale = 0;
            GameLocation location = Game1.player.currentLocation;
            if (location != null)
            {
                ApplySeason(ref envTemp, location, Game1.Date.TotalDays, Game1.getFarm().GetSeason().ToString());
                ApplyWeather(ref envTemp, location, Game1.Date.TotalDays);
                ApplyLocation(ref envTemp, location, Game1.Date.TotalDays, Game1.CurrentMineLevel);
                ApplyObjects(ref envTemp, location, PixelToTile(Game1.player.GetBoundingBox().Center.X), PixelToTile(Game1.player.GetBoundingBox().Center.Y));
                ApplyAmbient(ref envTemp, location);
            }
            else
            {
                envTemp = DefaultConsts.EnvTemp;
                fluctuationScale = 0;
                dayCycleScale = 0;
            }

            ApplyDayCycle(ref envTemp, Game1.timeOfDay);
            envTemp += fluctuation;

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