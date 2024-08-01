using System;
using StardewValley;

namespace Temperature.Framework.Misc
{
    public static class CalcHelper
    {
        public static float Distance(float aX, float aY, float bX, float bY)
            => MathF.Sqrt((aX - bX) * (aX - bX) + (aY - bY) * (aY - bY));

        public static float ParabolaWithCentralExtremum(float x, float extremumValue, float boundaryValue, float boundary1, float boundary2)
            => 4 * (boundaryValue - extremumValue) /
            ((boundary1 - boundary2) * (boundary1 - boundary2)) *
            (x - (boundary1 + boundary2) / 2) * (x - (boundary1 + boundary2) / 2) + extremumValue;

        public static float StraightThroughPoint(float x, float slope, float pointX, float pointY)
            => slope * (x - pointX) + pointY;

        public static float SinWithMinMax(float x, float minValue, float maxValue, float offset, float stretch)
            => (maxValue - minValue) / 2 * MathF.Sin((x - offset) * MathF.PI / stretch) + (maxValue + minValue) / 2;

        public static float ArctanWithLeftRightLim(float x, float leftLim, float rightLim, float offset, float stretch)
            => (rightLim - leftLim) / MathF.PI * MathF.Atan((x - offset) * MathF.PI / stretch) + (rightLim + leftLim) / 2;

        public static float PixelToTile(int pixel) => (float)pixel / Game1.tileSize;

        public static bool CheckIfItemIsActive(StardewValley.Object obj, bool activeType = false)
        {
            // check if the object is a machine for crafting (eg. Furnace, Charcoal Kiln)
            if (activeType)
            {
                if (obj.MinutesUntilReady > 0 && obj.heldObject.Value != null) return true;
                else return false;
            }
            else
            {
                // if not a machine for crafting (assuming furniture), check if said furniture is active
                if (obj.IsOn) return true;
                else return false;
            }
        }
    }
}
