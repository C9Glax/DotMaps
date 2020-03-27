using System;

namespace DotMaps.Utils
{
    public class Functions
    {
        public static float CalculateDistanceBetweenCoordinates(float lat1, float lon1, float lat2, float lon2)
        {
            int earthRadius = 6371;
            float differenceLat = DegreesToRadians(lat2 - lat1);
            float differenceLon = DegreesToRadians(lon2 - lon1);

            float lat1Rads = DegreesToRadians(lat1);
            float lat2Rads = DegreesToRadians(lat2);

            double a = Math.Sin(differenceLat / 2) * Math.Sin(differenceLat / 2) + Math.Sin(differenceLon / 2) * Math.Sin(differenceLon / 2) * Math.Cos(lat1Rads) * Math.Cos(lat2Rads);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Convert.ToSingle(earthRadius * c);
        }

        public static float DegreesToRadians(float degrees)
        {
            return degrees * Convert.ToSingle(Math.PI) / 180;
        }

        public static double AngleBetweenCoordinates(float lat1, float lon1, float lat2, float lon2)
        {
            float dy = lat2 - lat1;
            double dx = Math.Cos(Math.PI / 180 * lat1) * (lon2 - lon1);
            double angle = Math.Atan2(dy, dx);
            return angle;
        }
    }
}
