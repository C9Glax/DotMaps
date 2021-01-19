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
            return DistanceBetweenCoordinates(node1.position.lat, node1.position.lon, node2.position.lat, node2.position.lon);
        }

        public static double DistanceBetweenNodes(Graph.GraphNode node1, _3DNode node2)
        {
            return DistanceBetweenCoordinates(node1.position.lat, node1.position.lon, node2.lat, node2.lon);
        }

        public static double DistanceBetweenNodes(_3DNode node1, Graph.GraphNode node2)
        {
            return DistanceBetweenCoordinates(node1.lat, node1.lon, node2.position.lat, node2.position.lon);
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

        public static _2DNode _2DNodeFromGraphNode(Graph.GraphNode node, _3DNode cameraCenter, int scale)
        {
            return _2DNodeFrom3DNode(new _3DNode(node.position.lat, node.position.lon), cameraCenter, scale);
        }

        public static _2DNode _2DNodeFromGraphNodeAndVector(Graph.GraphNode node, _3DNode cameraCenter, Vector cameraCenterVector)
        {
            return _2DNodeFrom3DNodeAndCameraVector(new _3DNode(node.position.lat, node.position.lon), cameraCenter, cameraCenterVector);
        }

        public static _2DNode _2DNodeFrom3DNode(_3DNode node, _3DNode cameraCenter, int scale)
        {
            return _2DNodeFrom3DNodeAndCameraVector(node, cameraCenter, GetCameraVector(cameraCenter, scale));
        }

        public static _2DNode _2DNodeFrom3DNodeAndCameraVector(_3DNode node, _3DNode cameraCenter, Vector cameraCenterVector)
        {
            double radNodeLon = DegreesToRadians(node.lon);
            double radNodeLat = DegreesToRadians(node.lat);
            double cosRadNodeLat = Math.Cos(radNodeLat);
            //Vector to node
            Vector nodeVector = new Vector(
                Math.Cos(radNodeLon) * cosRadNodeLat,
                Math.Sin(radNodeLat),
                Math.Sin(radNodeLon) * cosRadNodeLat);

            if (cameraCenterVector.DotProductWith(nodeVector) == 0) //Node can't be projected onto plane
                Environment.Exit(-1);

            //Intersection between line through node and "camera"-plane
            double intersectionfactor = (Math.Pow(cameraCenterVector.x, 2) + Math.Pow(cameraCenterVector.y, 2) + Math.Pow(cameraCenterVector.z, 2)) /
                (cameraCenterVector.x * nodeVector.x + cameraCenterVector.y * nodeVector.y + cameraCenterVector.z * nodeVector.z);
            Vector vectorFromCenter = cameraCenterVector.Subtract(nodeVector.Scale(intersectionfactor));


            double radCameraLon = DegreesToRadians(cameraCenter.lon);
            Vector verticalVector = new Vector(Math.Sin(radCameraLon),
                0,
                -Math.Cos(radCameraLon));

            double angle = RadiansToDegrees(verticalVector.AngleTo(vectorFromCenter));
            double x = vectorFromCenter.length * Math.Sin(DegreesToRadians(90-angle));// / Math.Sin(DegreesToRadians(90)); = 1
            double y = vectorFromCenter.length * Math.Sin(DegreesToRadians(angle));// / Math.Sin(DegreesToRadians(90)); = 1
            if (node.lat > cameraCenter.lat) //TODO Fix the problem along x-axis
                y = -y;

            return new _2DNode((float)x, (float)y);
        }

        public static Vector GetCameraVector(_3DNode cameraCenter, int scale)
        {
            double radCameraLon = DegreesToRadians(cameraCenter.lon);
            double radCameraLat = DegreesToRadians(cameraCenter.lat);
            double cosRadCameraLat = Math.Cos(radCameraLat);
            //Camera Norm-Vector
            return new Vector(
                Math.Cos(radCameraLon) * cosRadCameraLat,
                Math.Sin(radCameraLat),
                Math.Sin(radCameraLon) * cosRadCameraLat).Scale(scale * earthRadius);
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
