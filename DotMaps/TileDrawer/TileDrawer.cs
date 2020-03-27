using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using DotMaps.Datastructures;
using DotMaps.Utils;
using System.Drawing;
using System.Drawing.Imaging;

namespace DotMaps
{
    class TileDrawer
    {

        public TileDrawer(string path)
        {

        }

        public static void CreateTilesFromGraph(string path, Graph graph)
        {
            Hashtable visible = new Hashtable();
            foreach (string type in File.ReadAllLines("visible.txt"))
                visible.Add(type.Split(',')[0], Convert.ToByte(type.Split(',')[1]));
            Hashtable colors = new Hashtable();
            foreach (string type in File.ReadAllLines("visible.txt"))
                switch ((string)type.Split(',')[2])
                {
                    case "Red":
                        visible.Add(type.Split(',')[0], new Pen(Color.Red,5));
                        break;
                    case "Orange":
                        visible.Add(type.Split(',')[0], new Pen(Color.Orange, 5));
                        break;
                    case "White":
                        visible.Add(type.Split(',')[0], new Pen(Color.White, 5));
                        break;
                    case "Gray":
                        visible.Add(type.Split(',')[0], new Pen(Color.Gray, 5));
                        break;
                    case "Yellow":
                        visible.Add(type.Split(',')[0], new Pen(Color.Yellow, 5));
                        break;
                    case "Green":
                        visible.Add(type.Split(',')[0], new Pen(Color.Green, 5));
                        break;
                    case "Blue":
                        visible.Add(type.Split(',')[0], new Pen(Color.Blue, 5));
                        break;
                    default:
                        visible.Add(type.Split(',')[0], new Pen(Color.Black, 5));
                        break;
                }

            float minLat = float.MaxValue, minLon = float.MaxValue, maxLat = float.MinValue, maxLon = float.MinValue, lonDiff, latDiff;
            foreach (Node node in graph.nodes.Values)
            {
                minLat = node.lat < minLat ? node.lat : minLat;
                minLon = node.lon < minLon ? node.lon : minLon;
                maxLat = node.lat > maxLat ? node.lat : maxLat;
                maxLon = node.lon > maxLon ? node.lon : maxLon;
            }

            for (byte level = 7; level >= 0; level--)
            {
                List<Node>[,] tiles = CreateGrid(graph, level);
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    Directory.CreateDirectory(@path + "\\" + x);
                    for (int y = 0; y < tiles.GetLength(1); y++)
                    {
                    Directory.CreateDirectory(@path + "\\" + x + "\\" + y);
                        using (Bitmap bmp = new Bitmap(1000 * level, 1000 * level))
                        {
                            using (Graphics graphics = Graphics.FromImage(bmp))
                            {
                                foreach (Node node in tiles[x, y])
                                {
                                    foreach (Connection connection in node.GetConnections())
                                    {
                                        if ((byte)visible[connection.type] <= level)
                                        {
                                            float pixelX = (Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, node.lon) - x) * 1000;
                                            float pixelY = (Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, node.lat, minLon) - y) * 1000;
                                            double angle = Functions.AngleBetweenCoordinates(node.lat, node.lon, connection.neighbor.lat, connection.neighbor.lon);
                                            double distance = connection.distance;
                                            Pen pen = (Pen)colors[connection.type];
                                            graphics.DrawLine(pen, pixelX, pixelY, (float)(Math.Sin(angle) * distance), (float)(Math.Cos(angle) * distance));
                                        }
                                    }
                                }
                            }
                            bmp.Save(@path + "\\" + x + "\\" + y + "\\" + level + ".png", ImageFormat.Png);
                        }
                    }
                }
            }
                
        }

        private static List<Node>[,] CreateGrid(Graph graph, int size)
        {
            float minLat = float.MaxValue, minLon = float.MaxValue, maxLat = float.MinValue, maxLon = float.MinValue, lonDiff, latDiff;

            foreach(Node node in graph.nodes.Values)
            {
                minLat = node.lat < minLat ? node.lat : minLat;
                minLon = node.lon < minLon ? node.lon : minLon;
                maxLat = node.lat > maxLat ? node.lat : maxLat;
                maxLon = node.lon > maxLon ? node.lon : maxLon;
            }
            lonDiff = maxLon - minLon;
            latDiff = maxLat - minLat;

            int amountX = (int)Math.Ceiling(lonDiff / size);
            int amountY = (int)Math.Ceiling(latDiff / size);

            List<Node>[,] grid = new List<Node>[amountX, amountY];
            for (int px = 0; px < grid.GetLength(0); px++)
                for (int py = 0; py < grid.GetLength(1); py++)
                    grid[px, py] = new List<Node>();

            int nodeX, nodeY;
            foreach (Node node in graph.nodes.Values)
            {
                nodeX = (int)Math.Floor(Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, node.lon) / size);
                nodeY = (int)Math.Floor(Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, node.lat, minLon) / size);
                grid[nodeX, nodeY].Add(node);
            }

            return grid;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
