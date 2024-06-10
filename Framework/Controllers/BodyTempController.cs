using System;
using Temperature.Framework.Data;
using Temperature.Framework.Misc;
namespace Temperature.Framework.Controllers
{
    public static class BodyTempController
    {
        public static float Update(float bodyTemp, float envTemp, ClothingModifiers hatData, ClothingModifiers shirtData, ClothingModifiers pantsData, ClothingModifiers bootsData)
        {
            float resultTemp = 0;
            LogHelper.Debug($"hatData   {hatData.ColdResistance} {hatData.HeatResistance}");
            LogHelper.Debug($"shirtData {shirtData.ColdResistance} {shirtData.HeatResistance}");
            LogHelper.Debug($"pantsData {pantsData.ColdResistance} {pantsData.HeatResistance}");
            LogHelper.Debug($"bootsData {bootsData.ColdResistance} {bootsData.HeatResistance}");

            float totalColdResistance = hatData.ColdResistance + shirtData.ColdResistance + pantsData.ColdResistance + bootsData.ColdResistance;
            float totalHeatResistance = hatData.HeatResistance + shirtData.HeatResistance + pantsData.HeatResistance + bootsData.HeatResistance;
            float defaultAvgComfyTemp = (DefaultConsts.MinComfyTemp + DefaultConsts.MaxComfyTemp) / 2;

            float minComfyTemp = defaultAvgComfyTemp + (DefaultConsts.MinComfyTemp - defaultAvgComfyTemp) * (totalColdResistance + 1);
            float maxComfyTemp = defaultAvgComfyTemp + (DefaultConsts.MaxComfyTemp - defaultAvgComfyTemp) * (totalHeatResistance + 1);

            if (envTemp >= maxComfyTemp)
            {
                resultTemp = 2 * MathF.Atan((envTemp - maxComfyTemp) * MathF.PI / 30) / MathF.PI;
            }
            else if (envTemp <= minComfyTemp)
            {
                resultTemp = 2 * MathF.Atan((envTemp - minComfyTemp) * MathF.PI / 30) / MathF.PI;
            }

            bodyTemp += (resultTemp - bodyTemp) / 10;
            LogHelper.Alert($"minComfyTemp {minComfyTemp}");
            LogHelper.Alert($"maxComfyTemp {maxComfyTemp}");
            LogHelper.Alert($"resultBodyTemp {resultTemp}");
            return bodyTemp;
        }
    }
}
