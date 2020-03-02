using UnityEngine;

namespace Battlehub.RTCommon
{
    public static class UnitsConverter 
    {
        public static string MetersToFeetInches(float meters)
        {
            int feet, inchesleft;
            MetersToFeetInches(meters, out feet, out inchesleft);
            return feet.ToString("0′ ") + inchesleft.ToString("0″").PadLeft(3, '0');
        }

        public static void MetersToFeetInches(float meters, out int feet, out int inchesleft)
        {
            float inchfeet = Mathf.Abs(meters) / 0.3048f;
            feet = (int)inchfeet;
            inchesleft = (int)((inchfeet - System.Math.Truncate(inchfeet)) / 0.08333f);
            if (inchesleft == 12)
            {
                inchesleft = 0;
                feet += 1;
            }
        }
    }
}
