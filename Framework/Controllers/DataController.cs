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
        public static GenericData<Dictionary<string, ClothesModifiers>> Clothes { get; set; } = new();
        public static GenericData<ObjectModifiers> Objects { get; set; } = new();
        public static float ObjectsMaxEffectiveRange = 0;
        public static void LoadData()
        {
            // BaseGame data
            string pathPrefix = AssetHelper.GetDataAssetsFolderPath();
            Seasons.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.SeasonsDataAssetFileName));
            Weather.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.WeatherDataAssetFileName));
            Locations.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.LocationsDataAssetFileName));
            Clothes.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ClothesDataAssetFileName));
            Objects.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ObjectsDataAssetFileName));

            // Mod data
            foreach (IModInfo _mod in ModEntry.Instance.Helper.ModRegistry.GetAll().ToList())
            {
                pathPrefix = Path.Combine(AssetHelper.GetDataAssetsFolderPath(true), _mod.Manifest.UniqueID);
                Seasons.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.SeasonsDataAssetFileName));
                Weather.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.WeatherDataAssetFileName));
                Locations.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.LocationsDataAssetFileName));
                Clothes.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ClothesDataAssetFileName));
                Objects.LoadData(Path.Combine(pathPrefix, AssetHelper.DataConstants.ObjectsDataAssetFileName));
            }

            foreach (var obj in Objects.Data)
            {
                if (obj.Value.EffectiveRange > ObjectsMaxEffectiveRange)
                    ObjectsMaxEffectiveRange = obj.Value.EffectiveRange;
            }
        }

        private static ClothesModifiers GetClothesData(string clothingName, string type = "")
        {
            if (clothingName == null) return null;
            return type switch
            {
                "Hat" => GetDataByIteration(clothingName, Clothes.Data.GetValueOrDefault(type)),
                "Shirt" => GetDataByIteration(clothingName, Clothes.Data.GetValueOrDefault(type)),
                "Pants" => GetDataByIteration(clothingName, Clothes.Data.GetValueOrDefault(type)),
                "Boots" => GetDataByIteration(clothingName, Clothes.Data.GetValueOrDefault(type)),
                _ => null,
            };
        }

        private static ClothesModifiers GetDataByIteration(string clothingName, Dictionary<string, ClothesModifiers> data)
        {
            LogHelper.Info($"clothingName {clothingName}");
            if (clothingName == null) return null;
            ClothesModifiers res = null;
            foreach (var x in data)
            {
                switch (x.Value.Pattern)
                {
                    case "match":
                        if (clothingName.Equals(x.Key))
                        {
                            res = x.Value;
                            LogHelper.Info($"match {x.Key}");
                        }
                        break;
                    case "prefix":
                        if (clothingName.StartsWith(x.Key))
                        {
                            res = x.Value;
                            LogHelper.Info($"prefix {x.Key}");
                        }
                        break;
                    case "postfix":
                        if (clothingName.EndsWith(x.Key))
                        {
                            res = x.Value;
                            LogHelper.Info($"postfix {x.Key}");
                        }
                        break;
                    case "contain":
                        if (clothingName.Contains(x.Key))
                        {
                            res = x.Value;
                            LogHelper.Info($"contain {x.Key}");
                        }
                        break;
                }
            }
            return res;
        }

        public static ClothesModifiers UpdateHatData(StardewValley.Objects.Hat hat)
        {
            if (hat != null)
                return GetClothesData(Game1.player.hat.Value.Name, "Hat") ?? new ClothesModifiers();
            else
                return new ClothesModifiers();
        }

        public static ClothesModifiers UpdateShirtData(StardewValley.Objects.Clothing shirt)
        {
            if (shirt != null)
                return GetClothesData(Game1.player.hat.Value.Name, "Shirt") ?? new ClothesModifiers();
            else
                return new ClothesModifiers();
        }

        public static ClothesModifiers UpdatePantsData(StardewValley.Objects.Clothing pants)
        {
            if (pants != null)
                return GetClothesData(Game1.player.hat.Value.Name, "Pants") ?? new ClothesModifiers();
            else
                return new ClothesModifiers();
        }

        public static ClothesModifiers UpdateBootsData(StardewValley.Objects.Boots boots)
        {
            if (boots != null)
                return GetClothesData(Game1.player.hat.Value.Name, "Boots") ?? new ClothesModifiers();
            else
                return new ClothesModifiers();
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