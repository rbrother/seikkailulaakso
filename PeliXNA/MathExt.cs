using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Brotherus
{
    public static class MathExt
    {
        public static float ToRadians(double angle)
        {
            return Convert.ToSingle(angle / 360.0 * 2 * Math.PI);
        }

        internal static double ToDegrees(double angle)
        {
            return angle / 2 / Math.PI * 360;
        }

        public static double Sqr(double value)
        {
            return value * value;
        }

    }
}
