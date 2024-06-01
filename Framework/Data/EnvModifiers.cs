namespace Temperature.Framework.Data
{
    public class EnvModifiers
    {
        public float additive { get; set; } = 0;
        public float multiplicative { get; set; } = 1;
        public float fixedValue { get; set; } = -274;
        public float timeDependentScale { get; set; } = 4;
        public float fluctuationScale { get; set; } = 1;
    }
}