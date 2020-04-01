using DotMaps.Datastructures;
using DotMaps.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace DotMaps
{
    class DrawEntireMap
    {
        static void Main(string[] args)
        {
            Console.WriteLine(".osm File to read");
            string path = Console.ReadLine();
            Console.WriteLine("output path");
            string newPath = Console.ReadLine();
            Console.WriteLine("Scale");
            int scale = Convert.ToInt32(Console.ReadLine());
            DrawEntireOSM(path, newPath, scale);
        }

        public static void DrawEntireOSM(string path, string newPath, int scale)
        {
            Hashtable pens = new Hashtable();
            foreach (string type in File.ReadAllLines("roadRender.txt"))
            {
                if (!type.StartsWith("//"))
                {
                    string key = type.Split(',')[0];
                    Color color = Color.FromName(type.Split(',')[2]);
                    int thickness = Convert.ToInt32(type.Split(',')[3]);
                    if (!pens.ContainsKey(key))
                        pens.Add(key, new Pen(color, thickness));
                }
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
                        _3dnodes.Add(id, new _3DNode(id, lat, lon));
                        minLat = lat < minLat ? lat : minLat;
                        minLon = lon < minLon ? lon : minLon;
                        maxLat = lat > maxLat ? lat : maxLat;
                        maxLon = lon > maxLon ? lon : maxLon;
                    }
                }
                reader.Close();
                stream.Close();
            }
            _3DNode center = new _3DNode(0, maxLat - minLat, maxLon - minLon);


            float minY = float.MaxValue, maxY = float.MinValue, minX = float.MaxValue, maxX = float.MinValue;
            Hashtable _2dnodes = new Hashtable();
            foreach (_3DNode _3dnode in _3dnodes.Values)
            {
                _2DNode newNode = Functions._2DNodeFrom3DNode(_3dnode, center, scale);
                _2dnodes.Add(newNode.id, newNode);
                minY = newNode.Y < minY ? newNode.Y : minY;
                minX = newNode.X < minX ? newNode.X : minX;
                maxY = newNode.Y > maxY ? newNode.Y : maxY;
                maxX = newNode.X > maxX ? newNode.X : maxX;
            }
            int width = (int)(maxX - minX + 1);
            int height = (int)(maxY - minY + 1);

            Console.WriteLine("        {0:F} <- {2:F} -> {1:F}", minX, maxX, width);
            Console.WriteLine("{0:F} \t +---------------->", minY);
            Console.WriteLine("        \t |");
            Console.WriteLine("{0:F}   \t |", height);
            Console.WriteLine("        \t |");
            Console.WriteLine("{0:F}\t\\|/", maxY);


            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    const byte UNKNOWN = 0, NODE = 1, WAY = 2, READING = 1, DONE = 0;
                    byte nodeType = UNKNOWN, state = DONE;
                    List<_2DNode> currentNodes = new List<_2DNode>();
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
                                        _2DNode[] nodes = currentNodes.ToArray();
                                        for (int i = 1; i < nodes.Length; i++)
                                        {
                                            g.FillEllipse(new SolidBrush(pen.Color), nodes[i - 1].X - minX - pen.Width / 2, nodes[i - 1].Y - minY - pen.Width / 2, pen.Width, pen.Width);
                                            g.DrawLine(pen, nodes[i - 1].X - minX, nodes[i - 1].Y - minY, nodes[i].X - minX, nodes[i].Y - minY);
                                        }
                                    }
                                    switch (reader.Name)
                                    {
                                        case "node":
                                            nodeType = NODE;
                                            break;
                                        case "way":
                                            currentNodes = new List<_2DNode>();
                                            currentWay = new Way(Convert.ToUInt64(reader.GetAttribute("id")));
                                            nodeType = WAY;
                                            break;
                                        default:
                                            nodeType = UNKNOWN;
                                            break;
                                    }
                                }
                                else if (reader.Depth == 2 && nodeType == WAY)
                                {
                                    state = READING;
                                    switch (reader.Name)
                                    {
                                        case "nd":
                                            UInt64 id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                            currentNodes.Add((_2DNode)_2dnodes[id]);
                                            break;
                                        case "tag":
                                            string key = reader.GetAttribute("k");
                                            currentWay.tags.Add(key, reader.GetAttribute("v"));
                                            break;
                                    }
                                }
                            }
                        }
                        reader.Close();
                    }
                }
                bmp.Save(newPath + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
