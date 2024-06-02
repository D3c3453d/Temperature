using System.Diagnostics;

namespace Temperature.Framework.Controllers
{
    public static class BodyTempController
    {
        public static void Update()
        {
            //LogHelper.Debug($"{MinComfortTemp} {MaxComfortTemp}");
            float envTempVal = ModEntry.PlayerData.EnvTemp;
            float targetBodyTemp = ModEntry.PlayerData.BodyTemp;
            //currently follow a segmented linear function (adjust to look good on desmos xd)
            if (envTempVal > ModEntry.PlayerData.MaxComfyTemp)
            {
                // if more than maximum comfort temp
                targetBodyTemp = ModEntry.Config.DefaultBodyTemp + ModEntry.Config.HighTemperatureSlope * (envTempVal - ModEntry.PlayerData.MaxComfyTemp);
            }
            else if (envTempVal < ModEntry.PlayerData.MinComfyTemp)
            {
                // if more than maximum comfort temp
                targetBodyTemp = ModEntry.Config.DefaultBodyTemp + ModEntry.Config.LowTemperatureSlope * (ModEntry.PlayerData.MinComfyTemp - envTempVal);
            }
            else
            {
                targetBodyTemp = ModEntry.Config.DefaultBodyTemp;
            }
            //gradual temp change instead of abrupted
            ModEntry.PlayerData.BodyTemp += (targetBodyTemp - ModEntry.PlayerData.BodyTemp) * ModEntry.Config.TemperatureChangeEasing;
            Misc.LogHelper.Warn(ModEntry.PlayerData.BodyTemp.ToString());
        }

        // internal void updateComfortTemp(string hat_name, string shirt_name, string pants_name, string boots_name)
        // {
        //     double DefaultAvgComfortTemp = (DefaultMinComfortTemp + DefaultMaxComfortTemp) / 2;
        //     double minComfortTempModifier = 1;
        //     double maxComfortTempModifier = 1;

        //     DataController.Clothes hatData = data.ClothingTempResistantDictionary.GetClothingData(hat_name, "hat");
        //     data.ClothingTempResistantData shirtData = data.ClothingTempResistantDictionary.GetClothingData(shirt_name, "shirt");
        //     data.ClothingTempResistantData pantsData = data.ClothingTempResistantDictionary.GetClothingData(pants_name, "pants");
        //     data.ClothingTempResistantData bootsData = data.ClothingTempResistantDictionary.GetClothingData(boots_name, "boots");

        //     minComfortTempModifier += ((hatData != null) ? hatData.coldInsulationModifier : 0)
        //         + ((shirtData != null) ? shirtData.coldInsulationModifier : 0)
        //         + ((pantsData != null) ? pantsData.coldInsulationModifier : 0)
        //         + ((bootsData != null) ? bootsData.coldInsulationModifier : 0);
        //     maxComfortTempModifier += ((hatData != null) ? hatData.heatInsulationModifier : 0)
        //         + ((shirtData != null) ? shirtData.heatInsulationModifier : 0)
        //         + ((pantsData != null) ? pantsData.heatInsulationModifier : 0)
        //         + ((bootsData != null) ? bootsData.heatInsulationModifier : 0);

        //     MinComfortTemp = DefaultAvgComfortTemp + (DefaultMinComfortTemp - DefaultAvgComfortTemp) * minComfortTempModifier;
        //     MaxComfortTemp = DefaultAvgComfortTemp + (DefaultMaxComfortTemp - DefaultAvgComfortTemp) * maxComfortTempModifier;
        // }
    }
}
