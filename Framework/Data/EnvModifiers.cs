namespace Temperature.Framework.Data
{
    public class EnvModifiers(float minValue = DefaultConsts.AbsoluteZero, float maxValue = DefaultConsts.AbsoluteZero, float dayCycleScale = 0, float fluctuationScale = 0, float additive = 0, float multiplicative = 1)
    {
        public float MinValue { get; set; } = minValue;
        public float MaxValue { get; set; } = maxValue;
        public float DayCycleScale { get; set; } = dayCycleScale;
        public float FluctuationScale { get; set; } = fluctuationScale;
        public float Additive { get; set; } = additive;
        public float Multiplicative { get; set; } = multiplicative;
    }
}