using System;
using DotMaps.Datastructures;

namespace DotMaps.Utils
{
    public class Functions
    {
        const int earthRadius = 6371;
        public static double DistanceBetweenNodes(_3DNode node1, _3DNode node2)
        {
            return DistanceBetweenCoordinates(node1.lat, node1.lon, node2.lat, node2.lon);
        }

        public static double DistanceBetweenCoordinates(float lat1, float lon1, float lat2, float lon2)
        {
            double differenceLat = DegreesToRadians(lat2 - lat1);
            double differenceLon = DegreesToRadians(lon2 - lon1);

            double lat1Rads = DegreesToRadians(lat1);
            double lat2Rads = DegreesToRadians(lat2);

            double a = Math.Sin(differenceLat / 2) * Math.Sin(differenceLat / 2) + Math.Sin(differenceLon / 2) * Math.Sin(differenceLon / 2) * Math.Cos(lat1Rads) * Math.Cos(lat2Rads);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        public static double DegreesToRadians(float degrees)
        {
            return degrees * Math.PI / 180;
        }
        public static double RadiansToDegrees(double Radians)
        {
            return (Radians * 180 / Math.PI + 360) % 360;
        }

        public static double AngleBetweenNodes(_3DNode node1, _3DNode node2)
        {
            return AngleBetweenCoordinates(node1.lat, node1.lon, node2.lat, node2.lon);
        }

        public static double AngleBetweenCoordinates(float lat1, float lon1, float lat2, float lon2)
        {
            double lt1 = DegreesToRadians(lat1);
            double ln1 = DegreesToRadians(lon1);
            double lt2 = DegreesToRadians(lat2);
            double ln2 = DegreesToRadians(lon2);

            double y = Math.Sin(ln2 - ln1) * Math.Cos(lt2);
            double x = Math.Cos(lt1) * Math.Sin(lt2) - Math.Sin(lt1) * Math.Cos(lt2) * Math.Cos(ln2 - ln1);
            double angle = Math.Atan2(y, x);

            return angle;
        }

        public static _2DNode _2DNodeFrom3DNode(_3DNode node, _3DNode cameraCenter, int scale)
        {
            //Node Position in 3D space
            double ax = Math.Cos(DegreesToRadians(node.lon)) * Math.Cos(DegreesToRadians(node.lat));
            double ay = Math.Sin(DegreesToRadians(node.lat));
            double az = Math.Sin(DegreesToRadians(node.lon)) * Math.Cos(DegreesToRadians(node.lat));

            //Camera Position
            //double cx = 0;
            //double cy = 0;
            //double cz = 0;

            //Camera Rotation
            double ox = DegreesToRadians(-cameraCenter.lat);
            double oy = DegreesToRadians(90 - cameraCenter.lon);
            //double oz = 0; 

            //Screen Position
            //double ex = 0;
            //double ey = 0;
            double ez = -(scale + 1);

            //double x = ax - cx;
            //double y = ay - cy;
            //double z = az;// - cz;
            //double dx = Math.Cos(oy) * (Math.Sin(oz) * y + Math.Cos(oz) * x) - Math.Sin(oy) * z;
            double dx = Math.Cos(oy) * ax - Math.Sin(oy) * az;
            //double dy = Math.Sin(ox) * (Math.Cos(oy) * z + Math.Sin(oy) * (Math.Sin(oz) * y + Math.Cos(oz) * x)) + Math.Cos(ox) * (Math.Cos(oz) * y - Math.Sin(oz) * x);
            double dy = Math.Sin(ox) * (Math.Cos(oy) * az + Math.Sin(oy) * ax) + Math.Cos(ox) * ay;
            //double dz = Math.Cos(ox) * (Math.Cos(oy) * z + Math.Sin(oy) * (Math.Sin(oz) * y + Math.Cos(oz) * x)) - Math.Sin(ox) * (Math.Cos(oz) * y - Math.Sin(oz) * x);
            double dz = Math.Cos(ox) * (Math.Cos(oy) * az + Math.Sin(oy) * ax) - Math.Sin(ox) * ay;

            //Node Position on Screen from center
            double bx = (ez / dz) * dx;// + ex;
            double by = (ez / dz) * dy;// + ey;

            return new _2DNode((float)bx, (float)by);
        }

        public static _3DNode _3DNodeFrom2DNode(_2DNode node, _3DNode center, int scale)
        {
            
        }
    }
}
