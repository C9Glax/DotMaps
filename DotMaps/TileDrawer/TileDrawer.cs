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
            Console.WriteLine("Path to .osm file");
            string path = Console.ReadLine();
            Console.WriteLine("Output folder");
            string newPath = Console.ReadLine();
            if(!newPath.EndsWith('\\'))
                newPath += '\\';
            Console.WriteLine("Tilesize (pixels) Recommended value = 100");
            int size = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Scale (1 km = x px at the center) Recommended value = 100");
            int scale = Convert.ToInt32(Console.ReadLine());
            DrawTiles(path, newPath, size, scale);
        }

        public static void DrawTiles(string path, string newPath, int tileSize, int scale)
        {
            Hashtable pens = new Hashtable();
            foreach (string type in File.ReadAllLines("roadRender.txt"))
                if (!type.StartsWith("//"))
                {
                    string key = type.Split(',')[0];
                    if (!pens.ContainsKey(key))
                        pens.Add(key, new Pen(Color.FromName(type.Split(',')[2]), Convert.ToInt32(type.Split(',')[3])));
                }

            Hashtable _3dnodes = new Hashtable();
            float minLat = float.MaxValue, maxLat = float.MinValue, minLon = float.MaxValue, maxLon = float.MinValue;

            StreamReader stream = new StreamReader(path, System.Text.Encoding.UTF8);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement && reader.Depth == 1 && reader.Name == "node")
                    {
                        ulong id = Convert.ToUInt64(reader.GetAttribute("id"));
                        float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace('.', ','));
                        float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace('.', ','));
                        _3dnodes.Add(id, new _3DNode(lat, lon));
                        minLat = lat < minLat ? lat : minLat;
                        minLon = lon < minLon ? lon : minLon;
                        maxLat = lat > maxLat ? lat : maxLat;
                        maxLon = lon > maxLon ? lon : maxLon;
                    }
                }
                reader.Close();
                stream.Close();
            }
            float latDiff = maxLat - minLat;
            float lonDiff = maxLon - minLon;
            _3DNode center = new _3DNode(minLat + latDiff / 2, minLon + lonDiff / 2);
            _2DNode topLeft = Functions._2DNodeFrom3DNode(new _3DNode(maxLat, minLon), center, scale);
            _2DNode bottomRight = Functions._2DNodeFrom3DNode(new _3DNode(minLat, maxLon), center, scale);
            Console.WriteLine("Top-Left x,y: {0}, {1}", topLeft.X, topLeft.Y);
            Console.WriteLine("Bottom-Right x,y: {0}, {1}", bottomRight.X, bottomRight.Y);
            int xAmount = (int)Math.Ceiling((bottomRight.X - topLeft.X) / tileSize);
            int yAmount = (int)Math.Ceiling((bottomRight.Y - topLeft.Y) / tileSize);
            float xOffset = -topLeft.X;
            float yOffset = -topLeft.Y;
            Console.WriteLine("LatDiff: {0} = {1} yTiles\t\tLonDiff: {2} = {3} xTiles", latDiff, yAmount, lonDiff, xAmount);

            List<Line>[,] grid = new List<Line>[xAmount, yAmount];
            for (int x = 0; x < xAmount; x++)
                for (int y = 0; y < yAmount; y++)
                    grid[x, y] = new List<Line>();

            const byte UNKNOWN = 0, WAY = 1, READING = 0, DONE = 1;
            byte nodeType = UNKNOWN, state = DONE;
            List<_3DNode> currentNodes = new List<_3DNode>();
            Way currentWay = new Way(0);
            stream = new StreamReader(path, System.Text.Encoding.UTF8);
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.Depth == 1)
                        {
                            if (state == READING && nodeType == WAY && currentWay.tags.ContainsKey("highway") && pens.ContainsKey((string)currentWay.tags["highway"]))
                            {
                                state = DONE;
                                Pen pen = (Pen)pens[(string)currentWay.tags["highway"]];
                                _3DNode[] nodes = currentNodes.ToArray();
                                for(int i = 1; i < nodes.Length; i++)
                                {
                                    _3DNode _3dfrom = nodes[i - 1];
                                    _3DNode _3dto = nodes[i];
                                    _2DNode _2dfrom = Functions._2DNodeFrom3DNode(_3dfrom, center, scale);
                                    _2DNode _2dto = Functions._2DNodeFrom3DNode(_3dto, center, scale);
                                    int minX = _2dfrom.X < _2dto.X ? (int)((_2dfrom.X + xOffset) / tileSize) : (int)((_2dto.X + xOffset) / tileSize);
                                    int maxX = _2dfrom.X > _2dto.X ? (int)((_2dfrom.X + xOffset) / tileSize) : (int)((_2dto.X + xOffset) / tileSize);
                                    int minY = _2dfrom.Y < _2dto.Y ? (int)((_2dfrom.Y + yOffset) / tileSize) : (int)((_2dto.Y + yOffset) / tileSize);
                                    int maxY = _2dfrom.Y > _2dto.Y ? (int)((_2dfrom.Y + yOffset) / tileSize) : (int)((_2dto.Y + yOffset) / tileSize);
                                    for (int x = minX; x <= maxX; x++)
                                        for (int y = minY; y <= maxY; y++)
                                            grid[x, y].Add(new Line(pen, _2dfrom, _2dto));
                                }
                            }
                            if (reader.Name == "way")
                            {
                                currentNodes.Clear();
                                currentWay.id = Convert.ToUInt64(reader.GetAttribute("id"));
                                currentWay.tags.Clear();
                                currentWay.nodes.Clear();
                                nodeType = WAY;
                            }
                            else
                                nodeType = UNKNOWN;
                        }
                        else if (reader.Depth == 2 && nodeType == WAY)
                        {
                            state = READING;
                            if(reader.Name == "nd")
                            {
                                ulong id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                currentNodes.Add((_3DNode)_3dnodes[id]);
                            }
                            else if(reader.Name == "tag")
                                currentWay.tags.Add(reader.GetAttribute("k"), reader.GetAttribute("v"));
                        }
                    }
                }
                reader.Close();
            }

            Directory.CreateDirectory(newPath);
            File.Copy(path, newPath + "map.osm", true);
            File.WriteAllText(newPath + "scale", scale.ToString());
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
                                //Console.WriteLine("X:{0:F}\tY:{1:F}\tTO\tX:{2:F}\tY:{3:F}", line.from.coordinateX, line.from.coordinateY, line.to.coordinateX, line.to.coordinateY);
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
