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

        public static double DegreesToRadians(double deg)
        {
            return deg * Math.PI / 180.0;
        }
        public static double RadiansToDegrees(double rad)
        {
            return (rad * 180.0 / Math.PI) % 360.0;
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

        /* OLD METHOD
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
        }*/

        public static _2DNode _2DNodeFrom3DNode(_3DNode node, _3DNode cameraCenter, int scale)
        {

            //Vector to node
            Vector nodeVector = new Vector(
                Math.Cos(DegreesToRadians(node.lon)) * Math.Cos(DegreesToRadians(node.lat)),
                Math.Sin(DegreesToRadians(node.lat)),
                Math.Sin(DegreesToRadians(node.lon)) * Math.Cos(DegreesToRadians(node.lat)));
            //Console.WriteLine("Node \tx: {0:0.000000000}\t\ty: {1:0.000000000}\t\tz: {2:0.000000000}", nodeVector.x, nodeVector.y, nodeVector.z);

            //Camera Norm-Vector
            Vector cameraVector = new Vector(
                Math.Cos(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat))).Scale(scale*earthRadius);
            //Console.WriteLine("Cam \tx: {0:000000000.00}\t\ty: {1:000000000.00}\t\tz: {2:000000000.00}", cameraVector.x, cameraVector.y, cameraVector.z);

            if (cameraVector.DotProductWith(nodeVector) == 0) //Node can't be projected onto plane
                Environment.Exit(-1);

            //Intersection between line through node and "camera"-plane
            double intersectionfactor = (Math.Pow(cameraVector.x, 2) + Math.Pow(cameraVector.y, 2) + Math.Pow(cameraVector.z, 2)) /
                (cameraVector.x * nodeVector.x + cameraVector.y * nodeVector.y + cameraVector.z * nodeVector.z);
            Vector vectorFromCenter = cameraVector.Subtract(nodeVector.Scale(intersectionfactor));


            Vector verticalVector = new Vector(Math.Sin(DegreesToRadians(cameraCenter.lon)),
                0,
                -Math.Cos(DegreesToRadians(cameraCenter.lon)));

            double angle = RadiansToDegrees(verticalVector.AngleTo(vectorFromCenter));
            double x = vectorFromCenter.length * Math.Sin(DegreesToRadians(90 - angle)) / Math.Sin(DegreesToRadians(90));
            double y = vectorFromCenter.length * Math.Sin(DegreesToRadians(angle)) / Math.Sin(DegreesToRadians(90));
            if (node.lat > cameraCenter.lat)
                y = -y;


            //Console.WriteLine("\tA: {0:000}\t\t\tx: {1:00000000}\t\ty: {2:00000000}", angle,x,y);

            return new _2DNode((float)x, (float)y);
        }

        /*
        public static _3DNode _3DNodeFrom2DNode(_2DNode node, _3DNode cameraCenter, int scale)
        {
            //Camera Norm-Vector
            Vector cameraVector = new Vector(
                Math.Cos(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat)));

            Vector vectorFromCenter = new Vector()
            {

            }

        }*/

        internal class Vector
        {
            public double x { get; }
            public double y { get; }
            public double z { get; }
            public double length { get; }
            public Vector(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.length = Math.Sqrt(x * x + y * y + z * z);
            }

            public Vector MultiplyWith(Vector secondVector)
            {
                return new Vector(this.x * secondVector.x, this.y * secondVector.y, this.z * secondVector.z);
            }

            public double AngleTo(Vector secondVector)
            {
                return Math.Acos(this.DotProductWith(secondVector) / (this.length * secondVector.length));
            }

            public double DotProductWith(Vector secondVector)
            {
                return this.x * secondVector.x + this.y * secondVector.y + this.z * secondVector.z;
            }

            public Vector CrossProductWith(Vector secondVector)
            {
                return new Vector(this.y * secondVector.z - this.y * secondVector.x,
                    this.z * secondVector.x - this.x * secondVector.z,
                    this.x * secondVector.y - this.y * secondVector.x);
            }

            public Vector Add(Vector secondVector)
            {
                return new Vector(this.x + secondVector.x,
                    this.y + secondVector.y,
                    this.z + secondVector.z);
            }

            public Vector Scale(double factor)
            {
                return new Vector(this.x * factor,
                    this.y * factor,
                    this.z * factor);
            }

            public Vector Subtract(Vector secondVector)
            {
                return new Vector(this.x - secondVector.x,
                    this.y - secondVector.y,
                    this.z - secondVector.z);
            }
        }
    }
}
