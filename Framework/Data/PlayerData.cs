namespace Temperature.Framework.Data
{
    public class PlayerData
    {
        public float EnvTemp { get; set; } = DefaultConsts.EnvTemp;
        public float BodyTemp { get; set; } = DefaultConsts.BodyTemp;
        public float MaxComfyTemp { get; set; } = DefaultConsts.MaxComfyTemp;
        public float MinComfyTemp { get; set; } = DefaultConsts.MinComfyTemp;
        public EnvModifiers CurrentSeasonData { get; set; } = new EnvModifiers(DefaultConsts.MaxSeasonTemp, DefaultConsts.MinSeasonTemp);
        public EnvModifiers CurrentWeatherData { get; set; } = new EnvModifiers();
        public EnvModifiers CurrentLocationData { get; set; } = new EnvModifiers(DefaultConsts.AbsoluteZero, DefaultConsts.AbsoluteZero, DefaultConsts.DayCycleScale, DefaultConsts.FluctuationScale);
        public ClothingModifiers CurrentHatData { get; set; } = new ClothingModifiers();
        public ClothingModifiers CurrentShirtData { get; set; } = new ClothingModifiers();
        public ClothingModifiers CurrentPantsData { get; set; } = new ClothingModifiers();
        public ClothingModifiers CurrentBootsData { get; set; } = new ClothingModifiers();
    }
}