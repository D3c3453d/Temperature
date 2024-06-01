using System.IO;

namespace Temperature.Framework.Misc
{
    public class AssetHelper
    {
        private const string AssetFolderName = "Assets";

        public static string GetBarAssetsFolderPath()
        {
            return Path.Combine(AssetFolderName, BarsConstants.BarAssetsFolderName);
        }

        public static string GetDataAssetsFolderPath(bool toTheCustom = false)
        {
            return Path.Combine(AssetFolderName, DataConstants.DataAssetsFolderName,
                                toTheCustom ? DataConstants.CustomDataFolderName : DataConstants.BaseGameDataFolderName);
        }

        public class BarsConstants
        {
            internal const string BarAssetsFolderName = "Bars";

            public const string EnvTempBarAssetFileName = "EnvTemp_Sprite.png";

            public const string BodyTempBarAssetFileName = "BodyTemp_Sprite.png";
        }

        public class DataConstants
        {
            internal const string DataAssetsFolderName = "Data";

            internal const string CustomDataFolderName = "Custom";

            public const string BaseGameDataFolderName = "BaseGame";

            public const string ClothesDataAssetFileName = "Clothes.json";
            public const string LocationsDataAssetFileName = "Locations.json";
            public const string ObjectsDataAssetFileName = "Objects.json";
            public const string SeasonsDataAssetFileName = "Seasons.json";
            public const string WeatherDataAssetFileName = "Weather.json";
        }
    }
}