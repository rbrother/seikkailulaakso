using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Brotherus
{
    public static class MathExt
    {
        public static float ToRadians(this float angle) {
            return ToRadians((double)angle);
        }

        public static float ToRadians(this double angle)
        {
            return Convert.ToSingle(angle / 360.0 * 2 * Math.PI);
        }

        public static double ToDegrees(this float angle) {
            return ToDegrees((double)angle);
        }

        internal static double ToDegrees(this double angle)
        {
            return angle / 2 / Math.PI * 360;
        }

        public static double Sqr(this double value)
        {
            return value * value;
        }

        public static float InLimits(this float value, float min, float max) {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

    }
}
