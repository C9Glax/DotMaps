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
            return Radians * 180 / Math.PI;
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

        public static _2DNode _2DNodeFrom3DNode(_3DNode node, _3DNode center, int resolution)
        {
            double ax = earthRadius * Math.Cos(DegreesToRadians(node.lat)) * Math.Cos(DegreesToRadians(node.lon)); //sphere
            double ay = earthRadius * Math.Cos(DegreesToRadians(node.lat)) * Math.Sin(DegreesToRadians(node.lon)); //sphere
            double az = earthRadius * Math.Sin(DegreesToRadians(node.lat)); //sphere

            double cx = 0;// earthRadius * (resolution + 1) * Math.Cos(DegreesToRadians(center.lat)) * Math.Cos(DegreesToRadians(center.lon)); //camera
            double cy = 0;// earthRadius * (resolution + 1) * Math.Cos(DegreesToRadians(center.lat)) * Math.Sin(DegreesToRadians(center.lon)); //camera
            double cz = 0;// earthRadius * (resolution + 1) * Math.Sin(DegreesToRadians(center.lat)); //camera

            double ox = 0; //cameraangle
            double oy = DegreesToRadians(node.lon);//cameraangle
            double oz = DegreesToRadians(node.lat);//cameraangle

            double ex = 0; //screenrelativetopinhole
            double ey = 0; //screenrelativetopinhole
            double ez = earthRadius * resolution; //screenrelativetopinhole

            double x = ax - cx;
            double y = ay - cy;
            double z = az - cz;

            double dx = Math.Cos(oy) * (Math.Sin(oz) * y + Math.Cos(oz) * x) - Math.Cos(oy) * z;
            double dy = Math.Sin(ox) * (Math.Cos(oy) * z + Math.Sin(oy) * (Math.Sin(oy) * y + Math.Cos(oz) * x)) + Math.Cos(ox) * (Math.Cos(oz) * y - Math.Sin(oz) * x);
            double dz = Math.Cos(ox) * (Math.Cos(oy) * z + Math.Sin(oy) * (Math.Sin(oz) * y + Math.Cos(oz) * x)) - Math.Sin(ox) * (Math.Cos(oz) * y - Math.Sin(oz) * x);

            double bx = ez / dz * dx + ex;
            double by = ez / dz * dy + ey;

            return new _2DNode(node.id, (float)-bx, (float)by);
        }


        /*
        public static int[] GridFromCoordinates(float lengthCellLateralEdgeClosestToEquator, float startLat, float startLon, float finishLat, float finishLon) {
            double angle = AngleBetweenCoordinates(startLat, startLon, finishLat, finishLon);
            double distance = CalculateDistanceBetweenCoordinates(startLat, startLon, finishLat, finishLon);
            int x = (int)(Math.Sin(angle) * distance / lengthCellLateralEdgeClosestToEquator);
            int y = (int)-(Math.Cos(angle) * distance / lengthCellLateralEdgeClosestToEquator);
            return new int[2] { x, y };
        }

        public static float[] PixelsFromCoordinates(int resolution, float maxLat, float minLon, float nodeLat, float nodeLon)
        {
            double angle = AngleBetweenCoordinates(maxLat, minLon, nodeLat, nodeLon);
            double distance = CalculateDistanceBetweenCoordinates(maxLat, minLon, nodeLat, nodeLon);
            float pixelX = (float)(Math.Sin(angle) * distance) * resolution/10;
            float pixelY = (float)-(Math.Cos(angle) * distance) * resolution/10;
            return new float[2] { pixelX, pixelY };
        }

        public static float[] GridCellMaxLatMinLon(float lengthCellLateralEdgeClosestToEquator, float gridMinLat, float gridMaxLat, float gridMinLon, int gridX, int gridY)
        {
            const float kmAtEquator = 111.699f;
            float cellMaxLat = gridMaxLat - (lengthCellLateralEdgeClosestToEquator / kmAtEquator) * gridY;
            float cellMinLon = gridMinLon + (float)Math.Cos(gridMinLat) * (lengthCellLateralEdgeClosestToEquator / kmAtEquator) * gridX;
            return new float[2] { cellMaxLat, cellMinLon };
        }

        public static List<Line>[,] EmptyGridFromBounds(float lengthCellLateralEdgeClosestToEquator, float minLat, float maxLat, float minLon, float maxLon)
        {
            double width = CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, maxLon);
            double height = CalculateDistanceBetweenCoordinates(minLat, minLon, maxLat, minLon);
            int amountX = (int)Math.Ceiling(width / lengthCellLateralEdgeClosestToEquator);
            int amountY = (int)Math.Ceiling(height / lengthCellLateralEdgeClosestToEquator);

            List<Line>[,] emptyGrid = new List<Line>[amountX, amountY];
            for (int x = 0; x < emptyGrid.GetLength(0); x++)
                for (int y = 0; y < emptyGrid.GetLength(1); y++)
                    emptyGrid[x, y] = new List<Line>();

            return emptyGrid;
        }*/
    }
}
