using System;
using System.Collections.Generic;
using StardewValley;
using Temperature.Framework.Data;
using SObject = StardewValley.Object;


namespace Temperature.Framework.Controllers
{
    public static class EnvTempController
    {
        private static bool fixedTemp = false;
        private static float fluctuation = 0;
        private static float timeDependentScale = new EnvModifiers().timeDependentScale;
        private static float fluctuationScale = new EnvModifiers().fluctuationScale;
        private static Random rand = new Random();

        private static float pixelToTile(int pixel)
        {
            return (float)pixel / Game1.tileSize;
        }

        private static float distance(float aX, float aY, float bX, float bY)
        {
            return MathF.Sqrt((aX - bX) * (aX - bX) + (aY - bY) * (aY - bY));
        }

        private static bool checkIfItemIsActive(SObject obj, int checkType = 0)
        {
            //check if the object is a machine for crafting (eg. Furnace, Charcoal Kiln)
            if (checkType == 1)
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

        private static void applySeason(GameLocation location)
        {
            // season temperature modifiers

            string season = location.GetSeason().ToString();
            EnvModifiers seasonData = DataController.Seasons.Data.GetValueOrDefault(season) ?? new EnvModifiers();
            ModEntry.PlayerData.EnvTemp *= seasonData.multiplicative;
        }

        private static void applyWeather(GameLocation location)
        {
            // weather temperature modifiers

            string weather = location.GetWeather().Weather;
            EnvModifiers weatherData = DataController.Weather.Data.GetValueOrDefault(weather) ?? new EnvModifiers();
            ModEntry.PlayerData.EnvTemp *= weatherData.multiplicative;
        }

        private static void applyLocation(GameLocation location)
        {
            // location temperature modifiers
            var locationData = DataController.Locations.Data.GetValueOrDefault(location.Name) ?? new EnvModifiers();
            ModEntry.PlayerData.EnvTemp += locationData.additive;
            ModEntry.PlayerData.EnvTemp *= locationData.multiplicative;
            if (locationData.fixedValue > -273)
            {
                ModEntry.PlayerData.EnvTemp = locationData.fixedValue;
                fixedTemp = true;
            }
            timeDependentScale = locationData.timeDependentScale;
            fluctuationScale = locationData.fluctuationScale;

            if (location.Name.Contains("UndergroundMine"))
            {
                int currentMineLevel = Game1.CurrentMineLevel;
                switch (currentMineLevel)
                {
                    case 77377:
                        ModEntry.PlayerData.EnvTemp = ModEntry.Config.DefaultEnvTemp; break;
                    case >= 121:
                        ModEntry.PlayerData.EnvTemp = ModEntry.Config.DefaultEnvTemp + 0.045f * currentMineLevel; break;
                    case >= 80:
                        ModEntry.PlayerData.EnvTemp = 1.1f * MathF.Pow(currentMineLevel - 60, 1.05f); break;
                    case >= 40:
                        ModEntry.PlayerData.EnvTemp = 0.03f * MathF.Pow(currentMineLevel - 60, 2) - 12; break;
                    case >= 0:
                        ModEntry.PlayerData.EnvTemp = ModEntry.Config.DefaultEnvTemp + 0.22f * currentMineLevel; break;
                }
                fixedTemp = true;
            }

            if (!location.IsOutdoors)
            {
                if (location.IsFarm)
                    ModEntry.PlayerData.EnvTemp +=
                    (ModEntry.Config.DefaultEnvTemp - ModEntry.PlayerData.EnvTemp) * ModEntry.Config.FarmIndoorTemperatureMultiplier;
                else
                    ModEntry.PlayerData.EnvTemp +=
                    (ModEntry.Config.DefaultEnvTemp - ModEntry.PlayerData.EnvTemp) * ModEntry.Config.IndoorTemperatureMultiplier;
            }
        }

        private static void applyObjects(GameLocation location, float playerTileX, float playerTileY)
        {
            // local temperature emitted by objects

            int proximityCheckBound = 6;
            List<SObject> nearbyObjects = new List<SObject>();
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
                            if (objectData.needActive && !checkIfItemIsActive(obj, objectData.activeType)) continue;

                            // prioritize ambient temp if it exceed device's core temp
                            if ((objectData.deviceType.Equals("heating") && objectData.coreTemp < ModEntry.PlayerData.EnvTemp) ||
                                (objectData.deviceType.Equals("cooling") && objectData.coreTemp > ModEntry.PlayerData.EnvTemp))
                                continue;

                            // dealing with target temp this.value here?
                            float dist = MathF.Max(distance(pixelToTile(obj.GetBoundingBox().Center.X), pixelToTile(obj.GetBoundingBox().Center.Y), playerTileX, playerTileY), 1);
                            if (dist <= objectData.effectiveRange)
                            {
                                float tempModifierEntry = (objectData.coreTemp - ModEntry.PlayerData.EnvTemp) / (15 * (dist - 1) / objectData.effectiveRange + 1);
                                ModEntry.PlayerData.EnvTemp += tempModifierEntry;
                            }
                        }
                    }
                }
        }

        private static void applyAmbient(GameLocation location)
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
                        if (!objectData.needActive || checkIfItemIsActive(obj, objectData.activeType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                // furniture
                foreach (SObject obj in location.furniture)
                {
                    ObjectModifiers objectData = DataController.Objects.Data.GetValueOrDefault(obj.Name);
                    if (objectData != null)
                        if (!objectData.needActive || checkIfItemIsActive(obj, objectData.activeType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                float area = location.Map.GetLayer("Back").Tiles.Array.Length;
                float power = 0;

                foreach (ObjectModifiers objectData in objectsData)
                {
                    //calculate indoor heating power base on core temp and range (assume full effectiveness if object is placed indoor)
                    if (objectData.deviceType.Equals("general"))
                    {
                        float perfectAmbientPower = area * ModEntry.Config.DefaultEnvTemp;
                        float maxPowerFromDevice = objectData.operationalRange * (objectData.effectiveRange * 2 + 1) * (objectData.effectiveRange * 2 + 1) * objectData.ambientCoefficient;
                        if (ModEntry.Config.DefaultEnvTemp > ModEntry.PlayerData.EnvTemp)
                            power = MathF.Min(perfectAmbientPower, power + maxPowerFromDevice);
                        else
                            power = MathF.Max(perfectAmbientPower, power - maxPowerFromDevice);
                    }
                    else power += (objectData.coreTemp - ModEntry.Config.DefaultEnvTemp) * (objectData.effectiveRange * 2 + 1) * (objectData.effectiveRange * 2 + 1) * objectData.ambientCoefficient;
                }
                ModEntry.PlayerData.EnvTemp += 0.5f * power / area;
            }
        }

        public static void Update()
        {
            // main func
            // OnSecondPassed
            ModEntry.PlayerData.EnvTemp = ModEntry.Config.DefaultEnvTemp;
            GameLocation location = Game1.player.currentLocation;
            fixedTemp = false;
            if (location != null)
            {
                applySeason(location);
                applyWeather(location);
                applyLocation(location);
                applyObjects(location, pixelToTile(Game1.player.GetBoundingBox().Center.X), pixelToTile(Game1.player.GetBoundingBox().Center.Y));
                applyAmbient(location);
            }
            else
            {
                ModEntry.PlayerData.EnvTemp = ModEntry.Config.DefaultEnvTemp;
                fixedTemp = true;
            }

            // day cycle
            float decTime = Game1.timeOfDay / 100 + Game1.timeOfDay % 100 / 60.0f;
            ModEntry.PlayerData.EnvTemp += fixedTemp ? 0 : MathF.Sin((decTime - 8.5f) / (MathF.PI * 1.2f)) * timeDependentScale;

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