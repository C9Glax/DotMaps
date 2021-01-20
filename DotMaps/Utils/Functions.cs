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

        public static double DistanceBetweenNodes(Graph.GraphNode node1, Graph.GraphNode node2)
        {
            return DistanceBetweenCoordinates(node1.coordinates.lat, node1.coordinates.lon, node2.coordinates.lat, node2.coordinates.lon);
        }

        public static double DistanceBetweenNodes(Graph.GraphNode node1, _3DNode node2)
        {
            return DistanceBetweenCoordinates(node1.coordinates.lat, node1.coordinates.lon, node2.lat, node2.lon);
        }

        public static double DistanceBetweenNodes(_3DNode node1, Graph.GraphNode node2)
        {
            return DistanceBetweenCoordinates(node1.lat, node1.lon, node2.coordinates.lat, node2.coordinates.lon);
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
            return rad * 180.0 / Math.PI;
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

        public static Vector _3DNodeToVector(_3DNode node)
        {
            double radLon = DegreesToRadians(node.lon);
            double radLat = DegreesToRadians(node.lat);
            double cosRadLat = Math.Cos(radLat);
            //Camera Norm-Vector
            return new Vector(
                Math.Cos(radLon) * cosRadLat,
                Math.Sin(radLat),
                Math.Sin(radLon) * cosRadLat);
        }

        public static _3DNode VectorTo3DNode(Vector vector)
        {
            float lat = (float)RadiansToDegrees(Math.Asin(vector.y));
            float lon = (float)RadiansToDegrees(Math.Asin(vector.z / Math.Cos(Math.Asin(vector.y))));
            return new _3DNode(lat, lon);
        }

        /*
        public static _3DNode _3DNodeFrom2DNode(_2DNode node, _3DNode cameraCenter, int scale)
        {
            //Camera Norm-Vector
            Vector cameraVector = new Vector(
                Math.Cos(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat))).Scale(scale * earthRadius);

            Vector xVector = new Vector(Math.Sin(DegreesToRadians(cameraCenter.lon)),
                0,
                -Math.Cos(DegreesToRadians(cameraCenter.lon)));
            Vector yVector = cameraVector.CrossProductWith(xVector);

            Vector vectorFromCenter = xVector.Scale(node.X).Add(yVector.Scale(-node.Y));

            Vector nodeVector = cameraVector.Add(vectorFromCenter);
            nodeVector = nodeVector.Scale(1 / nodeVector.length);

            double lat = RadiansToDegrees(Math.Asin(nodeVector.y));
            return new _3DNode((float)lat, (float)0);
        }*/
    }
}
