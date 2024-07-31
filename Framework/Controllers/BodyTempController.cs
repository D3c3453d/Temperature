using System;
using Temperature.Framework.Data;
using Temperature.Framework.Misc;
namespace Temperature.Framework.Controllers
{
    public static class BodyTempController
    {
        private static readonly float defaultAvgComfyTemp = (DefaultConsts.MinComfyTemp + DefaultConsts.MaxComfyTemp) / 2;
        public static float Update(float bodyTemp, float envTemp, ClothingModifiers hatData, ClothingModifiers shirtData, ClothingModifiers pantsData, ClothingModifiers bootsData)
        {
            float resultTemp = 0;
            LogHelper.Debug($"hatData   {hatData.ColdResistance} {hatData.HeatResistance}");
            LogHelper.Debug($"shirtData {shirtData.ColdResistance} {shirtData.HeatResistance}");
            LogHelper.Debug($"pantsData {pantsData.ColdResistance} {pantsData.HeatResistance}");
            LogHelper.Debug($"bootsData {bootsData.ColdResistance} {bootsData.HeatResistance}");

            float totalColdResistance = hatData.ColdResistance + shirtData.ColdResistance + pantsData.ColdResistance + bootsData.ColdResistance;
            float totalHeatResistance = hatData.HeatResistance + shirtData.HeatResistance + pantsData.HeatResistance + bootsData.HeatResistance;

            float minComfyTemp = DefaultConsts.MinComfyTemp + (DefaultConsts.MinComfyTemp - defaultAvgComfyTemp) * totalColdResistance;
            float maxComfyTemp = DefaultConsts.MaxComfyTemp + (DefaultConsts.MaxComfyTemp - defaultAvgComfyTemp) * totalHeatResistance;

            if (envTemp >= maxComfyTemp)
            {
                resultTemp = CalcHelper.ArctanWithLeftRightLim(envTemp, DefaultConsts.MinBodyTemp, DefaultConsts.MaxBodyTemp, maxComfyTemp, 30);
            }
            else if (envTemp <= minComfyTemp)
            {
                resultTemp = CalcHelper.ArctanWithLeftRightLim(envTemp, DefaultConsts.MinBodyTemp, DefaultConsts.MaxBodyTemp, minComfyTemp, 30);
            }

            float step = (resultTemp - bodyTemp) * DefaultConsts.BodyTempSlope;
            bodyTemp += MathF.Abs(step) < DefaultConsts.MinStep ? (resultTemp - bodyTemp) : step;
            LogHelper.Alert($"minComfyTemp {minComfyTemp}");
            LogHelper.Alert($"maxComfyTemp {maxComfyTemp}");
            LogHelper.Alert($"resultBodyTemp {resultTemp}");
            return bodyTemp;
        }
    }
}
