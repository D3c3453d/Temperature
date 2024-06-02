namespace Temperature.Framework.Data
{
    public class PlayerData
    {
        public float EnvTemp { get; set; } = ModEntry.Config.DefaultEnvTemp;
        public float BodyTemp { get; set; } = ModEntry.Config.DefaultBodyTemp;
        public float MaxComfyTemp { get; set; } = ModEntry.Config.DefaultMaxComfyTemp;
        public float MinComfyTemp { get; set; } = ModEntry.Config.DefaultMinComfyTemp;
    }
}