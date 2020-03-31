using System;
using System.Collections;
using DotMaps.Datastructures;
using System.Xml;

namespace DotMaps.Utils
{
    public class Importer
    {

        public static Graph ReadNodesFromOSMFileIntoGraph(string path)
        {
            Graph graph = new Graph();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader reader = XmlReader.Create(path, settings)) {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.Depth == 1)
                        {
                            switch (reader.Name)
                            {
                                case "node":
                                    UInt64 id = Convert.ToUInt64(reader.GetAttribute("id"));
                                    float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace('.', ','));
                                    float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace('.', ','));
                                    graph.nodes.Add(id, new _3DNode(id, lat, lon));
                                    break;
                                case "bounds":
                                    graph.minLat = Convert.ToSingle(reader.GetAttribute("minlat").Replace('.', ','));
                                    graph.minLon = Convert.ToSingle(reader.GetAttribute("minlon").Replace('.', ','));
                                    graph.maxLat = Convert.ToSingle(reader.GetAttribute("maxlat").Replace('.', ','));
                                    graph.maxLon = Convert.ToSingle(reader.GetAttribute("maxlon").Replace('.', ','));
                                    break;
                            }
                        }
                    }
                }
                reader.Close();
            }
            return graph;
        }

        public static void  ReadWaysFromOSMFileIntoConnections(string path, ref Graph graph)
        {
            Hashtable speeds = new Hashtable();
            foreach (string speed in System.IO.File.ReadAllLines("speeds.txt"))
                speeds.Add(speed.Split(',')[0], Convert.ToInt32(speed.Split(',')[1]));

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            const byte UNKNOWN = 0, NODE = 1, WAY = 2, READING = 1, DONE = 0;
            byte nodeType = UNKNOWN, state = DONE;

            Way currentWay = new Way(0);
            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.Depth == 1)
                        {
                            if (state == READING)
                            {
                                state = DONE;
                                if (nodeType == WAY)
                                {
                                    if (currentWay.tags.ContainsKey("highway"))
                                    {
                                        for (int i = 1; i < currentWay.nodes.Count; i++)
                                        {
                                            _3DNode fromNode = currentWay.nodes[i];
                                            _3DNode toNode = currentWay.nodes[i - 1];

                                            double distance = Functions.DistanceBetweenCoordinates(fromNode.lat, fromNode.lon, toNode.lat, toNode.lon);
                                            int speed = 50;
                                            if (currentWay.tags.ContainsKey("highway") && speeds.ContainsKey((string)currentWay.tags["highway"]))
                                                speed = (int)speeds[(string)currentWay.tags["highway"]];
                                            if (currentWay.tags.ContainsKey("maxspeed"))
                                                try
                                                {
                                                    speed = Convert.ToInt32((string)currentWay.tags["maxspeed"]);
                                                }
                                                catch (FormatException)
                                                {
                                                    Console.WriteLine("Maxspeed {0} not implemented", (string)currentWay.tags["maxspeed"]);
                                                }
                                            float timeNeeded = (float)distance / speed;

                                            string name = "";
                                            if (currentWay.tags.ContainsKey("ref"))
                                                name = (string)currentWay.tags["ref"];
                                            else if (currentWay.tags.ContainsKey("name"))
                                                name = (string)currentWay.tags["name"];

                                            toNode.AddConnection(new Connection(distance, timeNeeded, fromNode, (string)currentWay.tags["highway"], name));
                                            if (!currentWay.tags.ContainsKey("oneway") || ((string)currentWay.tags["oneway"]) == "no")
                                                fromNode.AddConnection(new Connection(distance, timeNeeded, toNode, (string)currentWay.tags["highway"], name));
                                        }
                                    }
                                }
                            }
                            switch (reader.Name)
                            {
                                case "node":
                                    nodeType = NODE;
                                    break;
                                case "way":
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
                                    currentWay.nodes.Add((_3DNode)graph.nodes[id]);
                                    break;
                                case "tag":
                                    string key = reader.GetAttribute("k");
                                    if (key.StartsWith("addr:") || key == "building" || key == "ref" || key == "highway" || key == "name" || key == "maxspeed" || key == "oneway")
                                        currentWay.tags.Add(key, reader.GetAttribute("v"));
                                    break;
                            }
                        }
                    }
                }
                reader.Close();
            }
        }

        public static Graph ReadNodesIntoNewGraph(Hashtable nodes)
        {
            Graph map = new Graph();
            return ReadNodesIntoGraph(nodes, map);
        }

        public static Graph ReadNodesIntoGraph(Hashtable nodes, Graph graph)
        {
            foreach (_3DNode node in nodes.Values)
                graph.nodes.Add(node.id, node);
            return graph;
        }
    }
}
