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

        public static _2DNode _2DNodeFromGraphNode(Graph.GraphNode node, _3DNode center, int scale)
        {
            return _2DNodeFrom3DNode(new _3DNode(node.position.lat, node.position.lon), center, scale);
        }

        public static _2DNode _2DNodeFrom3DNode(_3DNode node, _3DNode cameraCenter, int scale)
        {

            //Vector to node
            Vector nodeVector = new Vector(
                Math.Cos(DegreesToRadians(node.lon)) * Math.Cos(DegreesToRadians(node.lat)),
                Math.Sin(DegreesToRadians(node.lat)),
                Math.Sin(DegreesToRadians(node.lon)) * Math.Cos(DegreesToRadians(node.lat)));

            //Camera Norm-Vector
            Vector cameraVector = new Vector(
                Math.Cos(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lat)),
                Math.Sin(DegreesToRadians(cameraCenter.lon)) * Math.Cos(DegreesToRadians(cameraCenter.lat))).Scale(scale*earthRadius);

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

            return new _2DNode((float)x, (float)y);
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
