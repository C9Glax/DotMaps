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
            lat1 = DegreesToRadians(lat1);
            lon1 = DegreesToRadians(lon1);
            lat2 = DegreesToRadians(lat2);
            lon2 = DegreesToRadians(lon2);

            var y = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);
            var angle = Math.Atan2(y, x);

            return angle;
        }

        public static double RadiansToDegrees(double Radians)
        {
            return Radians * 180 / Math.PI;
        }
    }
}
