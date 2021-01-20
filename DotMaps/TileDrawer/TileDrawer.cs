using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using DotMaps.Datastructures;
using DotMaps.Utils;
using System.Drawing;
using System.IO;

namespace DotMaps.Tiles
{
    public class TileDrawer
    {
        static void Main(string[] args)
        {
            //Functions._2DNodeFrom3DNode(new _3DNode(0, -45), new _3DNode(0, 0), 100);
            Console.WriteLine("Path to .osm file");
            string path = Console.ReadLine();
            Console.WriteLine("Output folder");
            string newPath = Console.ReadLine();
            if(!newPath.EndsWith('\\'))
                newPath += '\\';
            Console.WriteLine("Tilesize (pixels) Recommended value = 100");
            int size = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Scale (1 km = x px at the center) Recommended value = 1000");
            int scale = Convert.ToInt32(Console.ReadLine());
            DrawTiles(path, newPath, size, scale);
        }

        public static void DrawTiles(string path, string newPath, int tileSize, int scale)
        {
            const byte UNKNOWN = 0, NODE = 1, WAY = 2, READINGNODES = 1, NODEREAD = 0;
            byte nodeType = UNKNOWN, state = NODEREAD;

            float minLat = float.MaxValue, maxLat = float.MinValue, minLon = float.MaxValue, maxLon = float.MinValue;

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            };
            Hashtable nodes = new Hashtable();
            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement && reader.Depth == 1 && reader.Name == "node")
                    {
                        ulong id = Convert.ToUInt64(reader.GetAttribute("id"));
                        float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace(".", ","));
                        float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace(".", ","));
                        nodes.Add(id, new _3DNode(lat, lon));
                        minLat = minLat < lat ? minLat : lat;
                        minLon = minLon < lon ? minLon : lon;
                        maxLat = maxLat > lat ? maxLat : lat;
                        maxLon = maxLon > lon ? maxLon : lon;
                    }
                }
            }
            float latDiff = maxLat - minLat;
            float lonDiff = maxLon - minLon;
            _3DNode center = new _3DNode(minLat + latDiff / 2, minLon + lonDiff / 2);
            Renderer renderer = new Renderer(center, scale);
            _2DNode topLeft = renderer.GetCoordinatesFromCenter(new _3DNode(maxLat, minLon));
            _2DNode bottomRight = renderer.GetCoordinatesFromCenter(new _3DNode(minLat, maxLon));
            float xOffset = -topLeft.X;
            float yOffset = -topLeft.Y;
            double width = bottomRight.X + xOffset;
            double height = bottomRight.Y + yOffset;
            int yAmount = (int)Math.Ceiling(height / tileSize);
            int xAmount = (int)Math.Ceiling(width / tileSize);
            Console.WriteLine("Top-Left\tx,y: {0}, {1}", topLeft.X, topLeft.Y);
            Console.WriteLine("Bottom-Right\tx,y: {0}, {1}", bottomRight.X, bottomRight.Y);
            Console.WriteLine("Height: {0}px => {2} Tiles \tWidth: {1}px => {3} Tiles", height, width, yAmount, xAmount);


            List<Line>[,] grid = new List<Line>[xAmount, yAmount];
            for (int x = 0; x < xAmount; x++)
                for (int y = 0; y < yAmount; y++)
                    grid[x, y] = new List<Line>();

            Hashtable pens = new Hashtable();
            foreach (string type in File.ReadAllLines("roadRender.txt"))
                if (!type.StartsWith("//"))
                {
                    string key = type.Split(',')[0];
                    if (!pens.ContainsKey(key))
                        pens.Add(key, new Pen(Color.FromName(type.Split(',')[2]), Convert.ToInt32(type.Split(',')[1])));
                }

            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                List<ulong> currentNodes = new List<ulong>();
                Way currentWay = new Way(0);
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.Depth == 1)
                        {
                            if (state == READINGNODES && nodeType == WAY)
                            {
                                state = NODEREAD;
                                if (currentWay.tags.ContainsKey("highway"))
                                {
                                    Pen pen = (Pen)pens[(string)currentWay.tags["highway"]];
                                    if(pen == null)
                                        pen = (Pen)pens["default"];
                                    for (int i = 1; i < currentNodes.Count; i++)
                                    {
                                        _2DNode _2dfrom = renderer.GetCoordinatesFromCenter((_3DNode)nodes[currentNodes[i - 1]]);
                                        _2DNode _2dto = renderer.GetCoordinatesFromCenter((_3DNode)nodes[currentNodes[i]]);
                                        //Console.WriteLine("FROM X  {0:0000000.00} + {1:0000000.00} => {2:0000000.00}\t\tY  {3:0000000.00} + {4:0000000.00} => {5:0000000.00}", _2dfrom.X, xOffset, _2dfrom.X + xOffset, _2dfrom.Y, yOffset, _2dfrom.Y + yOffset);
                                        //Console.WriteLine("TO   X  {0:0000000.00} + {1:0000000.00} => {2:0000000.00}\t\tY  {3:0000000.00} + {4:0000000.00} => {5:0000000.00}", _2dto.X, xOffset, _2dto.X + xOffset, _2dto.Y, yOffset, _2dto.Y + yOffset);
                                        int minX = _2dfrom.X < _2dto.X ? (int)Math.Floor((_2dfrom.X + xOffset) / tileSize) : (int)Math.Floor((_2dto.X + xOffset) / tileSize);
                                        int maxX = _2dfrom.X > _2dto.X ? (int)Math.Floor((_2dfrom.X + xOffset) / tileSize) : (int)Math.Floor((_2dto.X + xOffset) / tileSize);
                                        int minY = _2dfrom.Y < _2dto.Y ? (int)Math.Floor((_2dfrom.Y + yOffset) / tileSize) : (int)Math.Floor((_2dto.Y + yOffset) / tileSize);
                                        int maxY = _2dfrom.Y > _2dto.Y ? (int)Math.Floor((_2dfrom.Y + yOffset) / tileSize) : (int)Math.Floor((_2dto.Y + yOffset) / tileSize);
                                        for (int x = minX; x <= maxX; x++)
                                            for (int y = minY; y <= maxY; y++)
                                                if(x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
                                                    grid[x, y].Add(new Line(pen, _2dfrom, _2dto));
                                    }
                                }
                                /*else if (currentWay.tags.ContainsKey("addr:housenumber") && !neededNodesIds.Contains(currentNodes[0]))
                                {
                                    //Addresses?
                                }*/
                                /*foreach (string key in currentWay.tags.Keys)
                                {
                                    //Streetnames?
                                }*/
                            }
                            switch (reader.Name)
                            {
                                case "node":
                                    nodeType = NODE;
                                    break;
                                case "way":
                                    currentNodes.Clear();
                                    currentWay.tags.Clear();
                                    currentWay.id = Convert.ToUInt32(reader.GetAttribute("id"));
                                    nodeType = WAY;
                                    break;
                                default:
                                    nodeType = UNKNOWN;
                                    break;
                            }
                        }
                        else if (reader.Depth == 2 && nodeType == WAY)
                        {
                            state = READINGNODES;
                            switch (reader.Name)
                            {
                                case "nd":
                                    currentNodes.Add(Convert.ToUInt64(reader.GetAttribute("ref")));
                                    break;
                                case "tag":
                                    currentWay.tags.Add(reader.GetAttribute("k"), reader.GetAttribute("v"));
                                    break;
                            }
                        }
                    }
                }
            }

            Directory.CreateDirectory(newPath);
            File.Copy(path, newPath + "map.osm", true);
            File.WriteAllLines(newPath + "information", new string[] {
                    "scale:" + scale.ToString(),
                    "minlat:" + minLat.ToString(),
                    "maxlat:" + maxLat.ToString(),
                    "minlon:" + minLon.ToString(),
                    "maxlon:" + maxLon.ToString()
                });
            for (int x = 0; x < xAmount; x++)
            {
                Directory.CreateDirectory(newPath + "\\" + x);
                for(int y = 0; y < yAmount; y++)
                {
                    using (Bitmap bmp = new Bitmap(tileSize, tileSize))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            foreach (Line line in grid[x, y])
                            {
                                float tileOffsetX = tileSize * x;
                                float tileOffsetY = tileSize * y;
                                PointF pointFrom = new PointF(line.from.X + xOffset - tileOffsetX, line.from.Y + yOffset - tileOffsetY);
                                PointF pointTo = new PointF(line.to.X + xOffset - tileOffsetX, line.to.Y + yOffset - tileOffsetY);
                                g.FillEllipse(new SolidBrush(line.pen.Color), pointFrom.X - line.pen.Width / 2, pointFrom.Y - line.pen.Width / 2, line.pen.Width, line.pen.Width);
                                g.DrawLine(line.pen, pointFrom, pointTo);
                                g.FillEllipse(new SolidBrush(line.pen.Color), pointTo.X - line.pen.Width / 2, pointTo.Y - line.pen.Width / 2, line.pen.Width, line.pen.Width);
                            }
                        }
                        bmp.Save(newPath + "\\" + x + "\\" + y + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
        }
    }
}
