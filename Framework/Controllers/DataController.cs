using StardewModdingAPI;
using Temperature.Framework.Misc;
using Temperature.Framework.Data;
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
        public static GenericData<Dictionary<string, ClothModifiers>> Clothes { get; set; } = new();
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
    }
}