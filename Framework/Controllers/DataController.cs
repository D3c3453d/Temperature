using StardewModdingAPI;
using Temperature.Framework.Misc;
using Temperature.Framework.Data;
using StardewValley;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Temperature.Framework.Controllers
{
    static class DataController
    {
        public static GenericData<EnvModifiers> Seasons { get; set; } = new();
        public static GenericData<EnvModifiers> Weather { get; set; } = new();
        public static GenericData<EnvModifiers> Locations { get; set; } = new();
        public static GenericData<Dictionary<string, ClothingModifiers>> Clothing { get; set; } = new();
        public static GenericData<ObjectModifiers> Objects { get; set; } = new();
        public static float ObjectsMaxEffectiveRange = 0;
        public static void LoadData()
        {
            // BaseGame data
            string pathPrefix = AssetHelper.GetDataAssetsFolderPath();
            Seasons.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.SeasonsDataAssetFileName));
            Weather.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.WeatherDataAssetFileName));
            Locations.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.LocationsDataAssetFileName));
            Clothing.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ClothingDataAssetFileName));
            Objects.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ObjectsDataAssetFileName));

            // Mod data
            foreach (IModInfo _mod in ModEntry.Instance.Helper.ModRegistry.GetAll().ToList())
            {
                pathPrefix = Path.Combine(AssetHelper.GetDataAssetsFolderPath(true), _mod.Manifest.UniqueID);
                Seasons.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.SeasonsDataAssetFileName));
                Weather.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.WeatherDataAssetFileName));
                Locations.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.LocationsDataAssetFileName));
                Clothing.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ClothingDataAssetFileName));
                Objects.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ObjectsDataAssetFileName));
            }

            foreach (var obj in Objects.Data)
            {
                if (obj.Value.EffectiveRange > ObjectsMaxEffectiveRange)
                    ObjectsMaxEffectiveRange = obj.Value.EffectiveRange;
            }
        }

        private static ClothingModifiers GetClothingData(string clothingName, string type = "")
        {
            if (clothingName == null) return null;
            return type switch
            {
                "Hat" => GetDataByIteration(clothingName, Clothing.Data.GetValueOrDefault(type)),
                "Shirt" => GetDataByIteration(clothingName, Clothing.Data.GetValueOrDefault(type)),
                "Pants" => GetDataByIteration(clothingName, Clothing.Data.GetValueOrDefault(type)),
                "Boots" => GetDataByIteration(clothingName, Clothing.Data.GetValueOrDefault(type)),
                _ => null,
            };
        }

        private static ClothingModifiers GetDataByIteration(string clothingName, Dictionary<string, ClothingModifiers> data)
        {
            // LogHelper.Info($"clothingName {clothingName}");
            if (clothingName == null) return null;
            ClothingModifiers res = null;
            foreach (var x in data)
            {
                switch (x.Value.Pattern)
                {
                    case "Equals":
                        if (clothingName.Equals(x.Key))
                        {
                            res = x.Value;
                            // LogHelper.Info($"Equals {x.Key}");
                        }
                        break;
                    case "StartsWith":
                        if (clothingName.StartsWith(x.Key))
                        {
                            res = x.Value;
                            // LogHelper.Info($"StartsWith {x.Key}");
                        }
                        break;
                    case "EndsWith":
                        if (clothingName.EndsWith(x.Key))
                        {
                            res = x.Value;
                            // LogHelper.Info($"EndsWith {x.Key}");
                        }
                        break;
                    case "Contains":
                        if (clothingName.Contains(x.Key))
                        {
                            res = x.Value;
                            // LogHelper.Info($"Contains {x.Key}");
                        }
                        break;
                }
            }
            return res;
        }

        public static ClothingModifiers UpdateHatData(StardewValley.Objects.Hat hat)
        {
            if (hat != null)
                return GetClothingData(hat.Name, "Hat") ?? new ClothingModifiers();
            else
                return new ClothingModifiers();
        }

        public static ClothingModifiers UpdateShirtData(StardewValley.Objects.Clothing shirt)
        {
            if (shirt != null)
                return GetClothingData(shirt.Name, "Shirt") ?? new ClothingModifiers();
            else
                return new ClothingModifiers();
        }

        public static ClothingModifiers UpdatePantsData(StardewValley.Objects.Clothing pants)
        {
            if (pants != null)
                return GetClothingData(pants.Name, "Pants") ?? new ClothingModifiers();
            else
                return new ClothingModifiers();
        }

        public static ClothingModifiers UpdateBootsData(StardewValley.Objects.Boots boots)
        {
            if (boots != null)
                return GetClothingData(boots.Name, "Boots") ?? new ClothingModifiers();
            else
                return new ClothingModifiers();
        }

        public static EnvModifiers UpdateSeasonData(string currentSeason, string farmSeason)
        {
            if (currentSeason == farmSeason) currentSeason = DefaultConsts.FullSeasonName;

            return Seasons.Data.GetValueOrDefault(currentSeason) ??
                new EnvModifiers(DefaultConsts.MaxSeasonTemp, DefaultConsts.MinSeasonTemp);
        }

        public static EnvModifiers UpdateWeatherData(string weather)
        {
            return Weather.Data.GetValueOrDefault(weather) ?? new EnvModifiers();
        }

        public static EnvModifiers UpdateLocationData(string locationName)
        {
            return Locations.Data.GetValueOrDefault(locationName) ??
                new EnvModifiers(DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero, DefaultConsts.DayCycleScale, DefaultConsts.FluctuationScale);
        }
    }
}
