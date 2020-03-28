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

        public TileDrawer()
        {
            Console.WriteLine("Path to .osm file");
            Hashtable nodes = new Hashtable();
            List<Way> ways = new List<Way>();
            Console.WriteLine("Reading .osm");
            Reader.ReadOSMXml(Console.ReadLine(), ref nodes, ref ways);
            Console.WriteLine("Importing nodes");
            Graph graph = Importer.ReadNodesIntoNewGraph(nodes);
            Console.WriteLine("Importing ways");
            Importer.ReadWaysIntoGraph(ways, ref graph);
            Console.WriteLine("Path to save tiles:");
            CreateTilesFromGraph(Console.ReadLine(), graph);
        }

        const int resolution = 1000;

        public static void CreateTilesFromGraph(string path, Graph graph)
        {
            Hashtable visible = new Hashtable();
            foreach (string type in File.ReadAllLines("visible.txt"))
                visible.Add(type.Split(',')[0], Convert.ToByte(type.Split(',')[1]));
            Hashtable colors = new Hashtable();
            foreach (string type in File.ReadAllLines("visible.txt"))
            {
                if (!colors.ContainsKey(type.Split(',')[0]))
                {
                    switch ((string)type.Split(',')[2])
                    {
                        case "Red":
                            colors.Add(type.Split(',')[0], new Pen(Color.Red, 5));
                            break;
                        case "Orange":
                            colors.Add(type.Split(',')[0], new Pen(Color.Orange, 5));
                            break;
                        case "White":
                            colors.Add(type.Split(',')[0], new Pen(Color.White, 5));
                            break;
                        case "Gray":
                            colors.Add(type.Split(',')[0], new Pen(Color.Gray, 5));
                            break;
                        case "Yellow":
                            colors.Add(type.Split(',')[0], new Pen(Color.Yellow, 5));
                            break;
                        case "Green":
                            colors.Add(type.Split(',')[0], new Pen(Color.Green, 5));
                            break;
                        case "Blue":
                            colors.Add(type.Split(',')[0], new Pen(Color.Blue, 5));
                            break;
                        case "LightBlue":
                            colors.Add(type.Split(',')[0], new Pen(Color.LightBlue, 5));
                            break;
                        default:
                            colors.Add(type.Split(',')[0], new Pen(Color.Black, 5));
                            break;
                    }
                }
                
            }
                

            float minLat = float.MaxValue, minLon = float.MaxValue, maxLat = float.MinValue, maxLon = float.MinValue;
            foreach (Node node in graph.nodes.Values)
            {
                minLat = node.lat < minLat ? node.lat : minLat;
                minLon = node.lon < minLon ? node.lon : minLon;
                maxLat = node.lat > maxLat ? node.lat : maxLat;
                maxLon = node.lon > maxLon ? node.lon : maxLon;
            }

            for (byte level = 7; level > 0; level--)
            {
                Directory.CreateDirectory(@path + "\\" + level);
                List<Node>[,] tiles = CreateGrid(graph, level);
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    Directory.CreateDirectory(@path + "\\" + level + "\\" + x);
                    for (int y = 0; y < tiles.GetLength(1); y++)
                    {
                        List<Node> draw = new List<Node>();
                        for(int px = (x < 1) ? 0 : x - 1; px < x + 1 && px < tiles.GetLength(0); px++)
                            for (int py = (y < 1) ? 0 : y - 1; py < y + 1 && py < tiles.GetLength(1); py++)
                                draw.AddRange(tiles[px, py]);
                        using (Bitmap bmp = new Bitmap(resolution * level, resolution * level))
                        {
                            using (Graphics graphics = Graphics.FromImage(bmp))
                            {
                                foreach (Node node in draw)
                                {
                                    foreach (Connection connection in node.GetConnections())
                                    {
                                        if (connection.type != null)// && visible.ContainsKey(connection.type) && (byte)visible[connection.type] >= level)
                                        {
                                            float pixel1X = (Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, node.lon) - x) * resolution;
                                            float pixel1Y = (Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, node.lat, minLon) - y) * resolution;
                                            double angle = Functions.AngleBetweenCoordinates(node.lat, node.lon, connection.neighbor.lat, connection.neighbor.lon);
                                            double distance = connection.distance * resolution;
                                            float pixel2X = (float)(Math.Sin(angle) * distance + pixel1X);
                                            float pixel2Y = (float)(Math.Cos(angle) * distance + pixel1Y);

                                            Pen pen = (Pen)colors[connection.type];
                                            if (pen == null)
                                                pen = new Pen(Color.Green, 5);
                                            graphics.DrawLine(pen, pixel1X, resolution * level - pixel1Y, pixel2X, resolution * level - pixel2Y);
                                        }
                                    }
                                }
                            }
                            bmp.Save(@path + "\\" + level + "\\" + x + "\\" + y + ".png", ImageFormat.Png);
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
            latDiff = Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, maxLat, minLon);
            lonDiff = Functions.CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, maxLon);

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
            new TileDrawer();
        }
    }
}
