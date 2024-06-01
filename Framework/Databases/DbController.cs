using StardewModdingAPI;
using Temperature.Framework.Misc;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Temperature.Framework.Databases
{
    static class DbController
    {
        public static GenericDb<EnvModifiers> Seasons { get; set; } = new();
        public static GenericDb<EnvModifiers> Weather { get; set; } = new();
        public static GenericDb<EnvModifiers> Locations { get; set; } = new();
        public static GenericDb<Dictionary<string, ClothModifiers>> Clothes { get; set; } = new();
        public static GenericDb<ObjectModifiers> Objects { get; set; } = new();
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
        }
    }
}