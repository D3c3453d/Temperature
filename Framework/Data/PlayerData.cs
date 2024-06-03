namespace Temperature.Framework.Data
{
    public class PlayerData
    {
        public float EnvTemp { get; set; } = DefaultConsts.EnvTemp;
        public float BodyTemp { get; set; } = DefaultConsts.BodyTemp;
        public float MaxComfyTemp { get; set; } = DefaultConsts.MaxComfyTemp;
        public float MinComfyTemp { get; set; } = DefaultConsts.MinComfyTemp;
    }
}