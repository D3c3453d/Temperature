using System.IO;
using StardewModdingAPI;

namespace Temperature.Framework.Misc
{
    public static class AssetHelper
    {
        private const string AssetFolderName = "Assets";

        public static string GetBarAssetsFolderPath()
        {
            return Path.Combine(ModEntry.Instance.Helper.DirectoryPath, AssetFolderName, BarsConstants.BarAssetsFolderName);
        }

        public static string GetDataAssetsFolderPath(bool toTheCustom = false)
        {
            return Path.Combine(ModEntry.Instance.Helper.DirectoryPath, AssetFolderName, DataConstants.DataAssetsFolderName,
                                toTheCustom ? DataConstants.CustomDataFolderName : DataConstants.BaseGameDataFolderName);
        }

        public static class BarsConstants
        {
            internal const string BarAssetsFolderName = "Bars";

            public const string EnvTempBarAssetFileName = "EnvTemp_Sprite.png";

            public const string BodyTempBarAssetFileName = "BodyTemp_Sprite.png";
        }

        public static class DataConstants
        {
            internal const string DataAssetsFolderName = "Data";

            internal const string CustomDataFolderName = "Custom";

            public const string BaseGameDataFolderName = "BaseGame";

            public const string ClothingDataAssetFileName = "Clothing.json";
            public const string LocationsDataAssetFileName = "Locations.json";
            public const string ObjectsDataAssetFileName = "Objects.json";
            public const string SeasonsDataAssetFileName = "Seasons.json";
            public const string WeatherDataAssetFileName = "Weather.json";
        }
    }
}
