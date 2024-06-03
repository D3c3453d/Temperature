using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Temperature.Framework.Data;
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

        private static float SeasonCycleTemp(float maxTemp, float minTemp, int totalDays, int monthDuration)
        {
            return (maxTemp - minTemp) / 2 * MathF.Sin((totalDays - monthDuration / 2) * MathF.PI / (2 * monthDuration)) + (maxTemp + minTemp) / 2;
        }

        private static void ApplySeason(GameLocation location)
        {
            // season temperature modifiers
            string season = location.GetSeason().ToString();
            if (season == Game1.getFarm().GetSeason().ToString()) season = "Full";
            var seasonData = DataController.Seasons.Data.GetValueOrDefault(season) ??
                new EnvModifiers(DefaultConsts.MaxSeasonTemp, DefaultConsts.MinSeasonTemp);
            ModEntry.PlayerData.EnvTemp = SeasonCycleTemp(seasonData.MaxValue, seasonData.MinValue, Game1.Date.TotalDays, WorldDate.DaysPerMonth);
        }

        private static void ApplyWeather(GameLocation location)
        {
            // weather temperature modifiers

            string weather = location.GetWeather().Weather;
            EnvModifiers weatherData = DataController.Weather.Data.GetValueOrDefault(weather) ?? new EnvModifiers();
            ModEntry.PlayerData.EnvTemp += weatherData.Additive;
            ModEntry.PlayerData.EnvTemp *= weatherData.Multiplicative;
        }

        private static void ApplyLocation(GameLocation location)
        {
            // default location temperature modifiers
            switch (Game1.CurrentMineLevel)
            {
                case StardewValley.Locations.MineShaft.upperArea:
                    if (!location.IsOutdoors)
                    {
                        float indoorModifier;
                        if (location.IsFarm)
                            indoorModifier = (DefaultConsts.EnvTemp - ModEntry.PlayerData.EnvTemp) * ModEntry.Config.FarmIndoorTemperatureMultiplier;
                        else
                            indoorModifier = (DefaultConsts.EnvTemp - ModEntry.PlayerData.EnvTemp) * ModEntry.Config.IndoorTemperatureMultiplier;
                        ModEntry.PlayerData.EnvTemp += indoorModifier;
                    }
                    break;
                case StardewValley.Locations.MineShaft.quarryMineShaft:
                    ModEntry.PlayerData.EnvTemp = DefaultConsts.EnvTemp; break;
                case >= StardewValley.Locations.MineShaft.bottomOfMineLevel:
                    ModEntry.PlayerData.EnvTemp = DefaultConsts.EnvTemp + 0.045f * Game1.CurrentMineLevel; break;
                case >= StardewValley.Locations.MineShaft.mineLavaLevel:
                    ModEntry.PlayerData.EnvTemp = 1.3f * (Game1.CurrentMineLevel - 60); break;
                case >= StardewValley.Locations.MineShaft.mineFrostLevel:
                    ModEntry.PlayerData.EnvTemp = 0.03f * MathF.Pow(Game1.CurrentMineLevel - 60, 2) - 12; break;
                case > StardewValley.Locations.MineShaft.upperArea:
                    ModEntry.PlayerData.EnvTemp = DefaultConsts.EnvTemp + 0.22f * Game1.CurrentMineLevel; break;
            }

            // custom locatin temperature modifiers
            var locationData = DataController.Locations.Data.GetValueOrDefault(location.Name) ??
                new EnvModifiers(DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero, DefaultConsts.DayCycleScale, DefaultConsts.FluctuationScale);

            ModEntry.PlayerData.EnvTemp += locationData.Additive;
            ModEntry.PlayerData.EnvTemp *= locationData.Multiplicative;

            switch (locationData.MinValue, locationData.MaxValue)
            {
                case ( > DefaultConsts.AbsoluteZero, > DefaultConsts.AbsoluteZero):
                    ModEntry.PlayerData.EnvTemp = SeasonCycleTemp(locationData.MaxValue, locationData.MinValue, Game1.Date.TotalDays, WorldDate.DaysPerMonth);
                    break;
                case ( > DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero):
                    ModEntry.PlayerData.EnvTemp = SeasonCycleTemp(locationData.MinValue, locationData.MinValue, Game1.Date.TotalDays, WorldDate.DaysPerMonth);
                    break;
                case (DefaultConsts.AbsoluteZero, > DefaultConsts.AbsoluteZero):
                    ModEntry.PlayerData.EnvTemp = SeasonCycleTemp(locationData.MaxValue, locationData.MaxValue, Game1.Date.TotalDays, WorldDate.DaysPerMonth);
                    break;
            }

            dayCycleScale = locationData.DayCycleScale;
            fluctuationScale = locationData.FluctuationScale;
        }

        private static void ApplyObjects(GameLocation location, float playerTileX, float playerTileY)
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
                            if ((objectData.DeviceType.Equals("heating") && objectData.CoreTemp < ModEntry.PlayerData.EnvTemp) ||
                                (objectData.DeviceType.Equals("cooling") && objectData.CoreTemp > ModEntry.PlayerData.EnvTemp))
                                continue;

                            // dealing with target temp this.value here?
                            float dist = MathF.Max(Distance(PixelToTile(obj.GetBoundingBox().Center.X), PixelToTile(obj.GetBoundingBox().Center.Y), playerTileX, playerTileY), 1);
                            if (dist <= objectData.EffectiveRange)
                            {
                                float tempModifierEntry = (objectData.CoreTemp - ModEntry.PlayerData.EnvTemp) / (15 * (dist - 1) / objectData.EffectiveRange + 1);
                                ModEntry.PlayerData.EnvTemp += tempModifierEntry;
                            }
                        }
                    }
                }
        }

        private static void ApplyAmbient(GameLocation location)
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
                    if (objectData.DeviceType.Equals("general"))
                    {
                        float perfectAmbientPower = area * DefaultConsts.EnvTemp;
                        float maxPowerFromDevice = objectData.OperationalRange * (objectData.EffectiveRange * 2 + 1) * (objectData.EffectiveRange * 2 + 1) * objectData.AmbientCoefficient;
                        if (DefaultConsts.EnvTemp > ModEntry.PlayerData.EnvTemp)
                            power = MathF.Min(perfectAmbientPower, power + maxPowerFromDevice);
                        else
                            power = MathF.Max(perfectAmbientPower, power - maxPowerFromDevice);
                    }
                    else power += (objectData.CoreTemp - DefaultConsts.EnvTemp) * (objectData.EffectiveRange * 2 + 1) * (objectData.EffectiveRange * 2 + 1) * objectData.AmbientCoefficient;
                }
                ModEntry.PlayerData.EnvTemp += 0.5f * power / area;
            }
        }

        public static void Update()
        {
            // main func
            // OnSecondPassed
            ModEntry.PlayerData.EnvTemp = 0;
            GameLocation location = Game1.player.currentLocation;
            if (location != null)
            {
                ApplySeason(location);
                ApplyWeather(location);
                ApplyLocation(location);
                ApplyObjects(location, PixelToTile(Game1.player.GetBoundingBox().Center.X), PixelToTile(Game1.player.GetBoundingBox().Center.Y));
                ApplyAmbient(location);
            }
            else
            {
                ModEntry.PlayerData.EnvTemp = DefaultConsts.EnvTemp;
                fluctuationScale = 0;
                dayCycleScale = 0;
            }

            // day cycle
            float decTime = Game1.timeOfDay / 100 + Game1.timeOfDay % 100 / 60.0f;
            ModEntry.PlayerData.EnvTemp += MathF.Sin((decTime - 8.5f) / (MathF.PI * 1.2f)) * dayCycleScale;

            // fluctuation
            ModEntry.PlayerData.EnvTemp += fluctuation;
            Misc.LogHelper.Warn("EnvTemp " + ModEntry.PlayerData.EnvTemp);
        }

        public static void FluctuationUpdate()
        {
            // fluctuation
            // OnTimeChanged
            fluctuation = (float)rand.NextDouble() * fluctuationScale - 0.5f * fluctuationScale;
        }
    }
}