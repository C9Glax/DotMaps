using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using DotMaps.Datastructures;
using DotMaps.Utils;
using System.Drawing;
using System.IO;

namespace DotMaps
{
    public class TileDrawer
    {
        static void Main(string[] args)
        {
            Console.WriteLine(".osm File to read");
            string path = Console.ReadLine();
            Console.WriteLine("output path");
            string newPath = Console.ReadLine();
            TileDrawer.CreateTiles(path, newPath);
            //TileDrawer.CreateTiles(@"D:\Jann\Downloads\koeln_slim.osm", @"C:\Users\Jann\Desktop\test");
        }

        public static void CreateTiles(string path, string newPath)
        {
            Hashtable colors = new Hashtable();
            foreach (string type in File.ReadAllLines("visible.txt"))
            {
                if (!colors.ContainsKey(type.Split(',')[0]))
                {
                    switch ((string)type.Split(',')[2])
                    {
                        case "Red":
                            colors.Add(type.Split(',')[0], Color.Red);
                            break;
                        case "Orange":
                            colors.Add(type.Split(',')[0], Color.Orange);
                            break;
                        case "White":
                            colors.Add(type.Split(',')[0], Color.White);
                            break;
                        case "Gray":
                            colors.Add(type.Split(',')[0], Color.Gray);
                            break;
                        case "Yellow":
                            colors.Add(type.Split(',')[0], Color.Yellow);
                            break;
                        case "Green":
                            colors.Add(type.Split(',')[0], Color.Green);
                            break;
                        case "Blue":
                            colors.Add(type.Split(',')[0], Color.Blue);
                            break;
                        case "LightBlue":
                            colors.Add(type.Split(',')[0], Color.LightBlue);
                            break;
                        default:
                            colors.Add(type.Split(',')[0], Color.Black);
                            break;
                    }
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
                        UInt64 id = Convert.ToUInt64(reader.GetAttribute("id"));
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
            foreach(_3DNode _3dnode in _3dnodes.Values)
            {
                _2DNode newNode = Functions._2DNodeFrom3DNode(_3dnode, center, 5);
                _2dnodes.Add(newNode.id, newNode);
                minY = newNode.coordinateY < minY ? newNode.coordinateY : minY;
                minX = newNode.coordinateX < minX ? newNode.coordinateX : minX;
                maxY = newNode.coordinateY > maxY ? newNode.coordinateY : maxY;
                maxX = newNode.coordinateX > maxX ? newNode.coordinateX : maxX;
            }
            int width = (int)(maxX - minX + 1);
            int height = (int)(maxY - minY + 1);

            Console.WriteLine("{0:F}\t^", maxY);
            Console.WriteLine("       \t|");
            Console.WriteLine("{0:F}\t|", height);
            Console.WriteLine("       \t|");
            Console.WriteLine("{0:F}\t+---------------->", minY);
            Console.WriteLine("    {0:F}<-{2:F}->{1:F}", minX, maxX, width);
            

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using(Graphics g = Graphics.FromImage(bmp))
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
                                    if (state == READING && nodeType == WAY && currentWay.tags.ContainsKey("highway"))
                                    {
                                        Pen pen = new Pen((Color)colors[(string)currentWay.tags["highway"]], 1);
                                        state = DONE;
                                        _2DNode[] nodes = currentNodes.ToArray();
                                        for(int i = 1; i < nodes.Length; i++)
                                        {
                                            g.DrawLine(pen, nodes[i - 1].coordinateX - minX, nodes[i - 1].coordinateY - minY, nodes[i].coordinateX - minX, nodes[i].coordinateY - minY);
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
                                            if (key == "highway" && !colors.ContainsKey(reader.GetAttribute("v")))
                                                colors.Add(reader.GetAttribute("v"), Color.Black);
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
