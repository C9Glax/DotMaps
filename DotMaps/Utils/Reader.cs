using System.Xml;
using DotMaps.Datastructures;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DotMaps.Utils
{
    public class Reader
    {
        public static string ReadOSMXml(string path, ref Hashtable nodes, ref List<Way> ways)
        {
            const byte UNKNOWN = 0, NODE = 1, WAY = 2, RELATION = 3;

            string bounds = "";

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            XmlReader reader = XmlReader.Create(path, settings);

            Way currentWay = new Way(0);
            byte state = UNKNOWN;

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.Depth == 1)
                        switch (reader.Name)
                        {
                            case "node":
                                state = NODE;
                                UInt64 id = Convert.ToUInt64(reader.GetAttribute("id"));
                                float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace('.',','));
                                float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace('.', ','));
                                nodes.Add(id, new Node(id, lat, lon));
                                break;
                            case "way":
                                currentWay = new Way(Convert.ToUInt64(reader.GetAttribute("id")));
                                ways.Add(currentWay);
                                state = WAY;
                                break;
                            case "relation":
                                state = RELATION;
                                break;
                            case "bounds":
                                state = UNKNOWN;
                                bounds = "<bounds minlat=\"" + reader.GetAttribute("minlat") + "\" minlon=\"" + reader.GetAttribute("minlon") + "\" maxlat=\"" + reader.GetAttribute("maxlat") + "\" maxlon=\"" + reader.GetAttribute("maxlon") + "\" />";
                                break;
                            default:
                                state = UNKNOWN;
                                break;
                        }
                    else if (reader.Depth == 2 && state == WAY)
                    {
                        switch (reader.Name)
                        {
                            case "nd":
                                UInt64 id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                currentWay.nodes.Add(id);
                                break;
                            case "tag":
                                string key = reader.GetAttribute("k");
                                if (key.StartsWith("addr:") || key == "building" || key == "highway")
                                    currentWay.tags.Add(key, reader.GetAttribute("v"));
                                break;
                        }
                    }
                }
            }
            reader.Close();

            return bounds;
        }

        public static Graph ReadNodesIntoGraph(Hashtable nodes)
        {
            Graph map = new Graph();
            foreach (Node node in nodes.Values)
                map.nodes.Add(node.id, node);
            return map;
        }
    }
}
