using System;
using System.Collections.Generic;
using StardewValley;
using Temperature.Framework.Databases;
using SObject = StardewValley.Object;


namespace Temperature.Framework.Moduls
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
            return (float)Math.Sqrt((aX - bX) * (aX - bX) + (aY - bY) * (aY - bY));
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
            EnvModifiers seasonData = DbController.Seasons.Data.GetValueOrDefault(season) ?? new EnvModifiers();
            ModEntry.Data.ActualEnvTemp *= seasonData.multiplicative;
        }

        private static void applyWeather(GameLocation location)
        {
            // weather temperature modifiers

            string weather = location.GetWeather().Weather;
            EnvModifiers weatherData = DbController.Weather.Data.GetValueOrDefault(weather) ?? new EnvModifiers();
            ModEntry.Data.ActualEnvTemp *= weatherData.multiplicative;
        }

        private static void applyLocation(GameLocation location)
        {
            // location temperature modifiers
            var locationData = DbController.Locations.Data.GetValueOrDefault(location.Name) ?? new EnvModifiers();
            ModEntry.Data.ActualEnvTemp += locationData.additive;
            ModEntry.Data.ActualEnvTemp *= locationData.multiplicative;
            if (locationData.fixedValue > -273)
            {
                ModEntry.Data.ActualEnvTemp = locationData.fixedValue;
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
                        ModEntry.Data.ActualEnvTemp = ModEntry.Data.InitialEnvTemp; break;
                    case >= 121:
                        ModEntry.Data.ActualEnvTemp = ModEntry.Data.InitialEnvTemp + 0.045f * currentMineLevel; break;
                    case >= 80:
                        ModEntry.Data.ActualEnvTemp = 1.1f * (float)Math.Pow(currentMineLevel - 60, 1.05); break;
                    case >= 40:
                        ModEntry.Data.ActualEnvTemp = 0.03f * (float)Math.Pow(currentMineLevel - 60, 2) - 12; break;
                    case >= 0:
                        ModEntry.Data.ActualEnvTemp = ModEntry.Data.InitialEnvTemp + 0.22f * currentMineLevel; break;
                }
                fixedTemp = true;
            }

            if (!location.IsOutdoors)
            {
                if (location.IsFarm)
                    ModEntry.Data.ActualEnvTemp +=
                    (ModEntry.Data.InitialEnvTemp - ModEntry.Data.ActualEnvTemp) * ModEntry.Config.FarmIndoorTemperatureMultiplier;
                else
                    ModEntry.Data.ActualEnvTemp +=
                    (ModEntry.Data.InitialEnvTemp - ModEntry.Data.ActualEnvTemp) * ModEntry.Config.IndoorTemperatureMultiplier;
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
                        ObjectModifiers objectData = DbController.Objects.Data.GetValueOrDefault(obj.Name);
                        if (objectData != null)
                        {
                            nearbyObjects.Add(obj);

                            // skip inactive objects, that need to be activated
                            if (objectData.needActive && !checkIfItemIsActive(obj, objectData.activeType)) continue;

                            // prioritize ambient temp if it exceed device's core temp
                            if ((objectData.deviceType.Equals("heating") && objectData.coreTemp < ModEntry.Data.ActualEnvTemp) ||
                                (objectData.deviceType.Equals("cooling") && objectData.coreTemp > ModEntry.Data.ActualEnvTemp))
                                continue;

                            // dealing with target temp this.value here?
                            float dist = Math.Max(distance(pixelToTile(obj.GetBoundingBox().Center.X), pixelToTile(obj.GetBoundingBox().Center.Y), playerTileX, playerTileY), 1);
                            if (dist <= objectData.effectiveRange)
                            {
                                float tempModifierEntry = (objectData.coreTemp - ModEntry.Data.ActualEnvTemp) / (15 * (dist - 1) / objectData.effectiveRange + 1);
                                ModEntry.Data.ActualEnvTemp += tempModifierEntry;
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
                    ObjectModifiers objectData = DbController.Objects.Data.GetValueOrDefault(obj.Name);
                    if (objectData != null)
                        if (!objectData.needActive || checkIfItemIsActive(obj, objectData.activeType)) // skips inactive objects, that need to be activated
                            objectsData.Add(objectData);
                }

                // furniture
                foreach (SObject obj in location.furniture)
                {
                    ObjectModifiers objectData = DbController.Objects.Data.GetValueOrDefault(obj.Name);
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
                        float perfectAmbientPower = area * ModEntry.Data.InitialEnvTemp;
                        float maxPowerFromDevice = objectData.operationalRange * (objectData.effectiveRange * 2 + 1) * (objectData.effectiveRange * 2 + 1) * objectData.ambientCoefficient;
                        if (ModEntry.Data.InitialEnvTemp > ModEntry.Data.ActualEnvTemp)
                            power = Math.Min(perfectAmbientPower, power + maxPowerFromDevice);
                        else
                            power = Math.Max(perfectAmbientPower, power - maxPowerFromDevice);
                    }
                    else power += (objectData.coreTemp - ModEntry.Data.InitialEnvTemp) * (objectData.effectiveRange * 2 + 1) * (objectData.effectiveRange * 2 + 1) * objectData.ambientCoefficient;
                }
                ModEntry.Data.ActualEnvTemp += 0.5f * power / area;
            }
        }

        public static void Update()
        {
            // main func
            // OnSecondPassed
            ModEntry.Data.ActualEnvTemp = ModEntry.Data.InitialEnvTemp;
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
                ModEntry.Data.ActualEnvTemp = ModEntry.Data.InitialEnvTemp;
                fixedTemp = true;
            }

            // day cycle
            float decTime = Game1.timeOfDay / 100 + Game1.timeOfDay % 100 / 60.0f;
            ModEntry.Data.ActualEnvTemp += fixedTemp ? 0 : (float)Math.Sin((decTime - 8.5) / (Math.PI * 1.2)) * timeDependentScale;

            // fluctuation
            ModEntry.Data.ActualEnvTemp += fluctuation;
            Common.Debugger.Log(ModEntry.Data.ActualEnvTemp.ToString(), "Warn");
        }

        public static void FluctuationUpdate()
        {
            // fluctuation
            // OnTimeChanged
            fluctuation = (float)rand.NextDouble() * fluctuationScale - 0.5f * fluctuationScale;
        }
    }
}