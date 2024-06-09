using System;
using System.Collections.Generic;
using StardewValley;
using Temperature.Framework.Data;
using Temperature.Framework.Misc;
namespace Temperature.Framework.Controllers
{
    public static class BodyTempController
    {
        public static float Update(float bodyTemp, float envTemp)
        {
            float resultTemp = 0;
            string hat_name = "", shirt_name = "", pants_name = "", boots_name = "";
            if (Game1.player.hat.Value != null) hat_name = Game1.player.hat.Value.Name;
            if (Game1.player.shirtItem.Value != null) shirt_name = Game1.player.shirtItem.Value.Name;
            if (Game1.player.pantsItem.Value != null) pants_name = Game1.player.pantsItem.Value.Name;
            if (Game1.player.boots.Value != null) boots_name = Game1.player.boots.Value.Name;
            var hatData = DataController.GetClothingData(hat_name, "Hat") ?? new ClothModifiers();
            var shirtData = DataController.GetClothingData(shirt_name, "Shirt") ?? new ClothModifiers();
            var pantsData = DataController.GetClothingData(pants_name, "Pants") ?? new ClothModifiers();
            var bootsData = DataController.GetClothingData(boots_name, "Boots") ?? new ClothModifiers();

            // LogHelper.Error($"hatData   {hatData.ColdResistance} {hatData.HeatResistance}");
            // LogHelper.Error($"shirtData {shirtData.ColdResistance} {shirtData.HeatResistance}");
            // LogHelper.Error($"pantsData {pantsData.ColdResistance} {pantsData.HeatResistance}");
            // LogHelper.Error($"bootsData {bootsData.ColdResistance} {bootsData.HeatResistance}");

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
            // LogHelper.Alert($"minComfyTemp {minComfyTemp}");
            // LogHelper.Alert($"maxComfyTemp {maxComfyTemp}");
            // LogHelper.Alert($"resultTemp {resultTemp}");
            // LogHelper.Alert($"bodyTemp {bodyTemp}");
            return bodyTemp;
        }
    }
}
