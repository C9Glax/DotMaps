using System;
using System.Collections;
using DotMaps.Datastructures;
using System.Xml;
using System.Collections.Generic;
using System.IO;

namespace DotMaps.Utils
{
    public class Importer
    {
        public static void  ReadWaysFromOSMFileIntoConnections(string path, ref Graph graph)
        {
            const byte UNKNOWN = 0, NODE = 1, WAY = 2, READING = 1, DONE = 0;
            byte nodeType = UNKNOWN, state = DONE;

            List<string> copykeys = new List<string>();
            foreach (string key in File.ReadAllLines("copykeys.txt"))
                copykeys.Add(key);
            Hashtable speeds = new Hashtable();
            foreach (string speed in File.ReadAllLines("speeds.txt"))
                speeds.Add(speed.Split(',')[0], Convert.ToInt32(speed.Split(',')[1]));

            Dictionary<ulong, Graph.GraphNode> tempNodes = new Dictionary<ulong, Graph.GraphNode>();

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            };

            uint wayCount = 0;
            Way currentWay = new Way(wayCount);
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
                                        Graph.GraphNode start = currentWay.nodes[0];
                                        Graph.GraphNode end = currentWay.nodes[currentWay.nodes.Count - 1];
                                        if (!graph.ContainsNode(start.id))
                                            graph.AddNode(start);
                                        if (!graph.ContainsNode(end.id))
                                            graph.AddNode(end);

                                        List<_3DNode> coordinates = new List<_3DNode>();
                                        double distance = 0.0;
                                        for (int i = 1; i < currentWay.nodes.Count; i++)
                                        {
                                            Graph.GraphNode fromNode = currentWay.nodes[i];
                                            Graph.GraphNode toNode = currentWay.nodes[i - 1];
                                            distance += Functions.DistanceBetweenCoordinates(fromNode.lat, fromNode.lon, toNode.lat, toNode.lon);
                                            coordinates.Add(new _3DNode(fromNode.lat, fromNode.lon));
                                        }
                                        int speed = (int)speeds["default"];
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
                                            name = (string)currentWay.tags["name"]; ;

                                        end.AddConnection(new Graph.Connection(distance, timeNeeded, start, name, coordinates));
                                        if (!currentWay.tags.ContainsKey("oneway") || ((string)currentWay.tags["oneway"]) == "no")
                                            start.AddConnection(new Graph.Connection(distance, timeNeeded, end, name, coordinates));
                                    }
                                }
                            }
                            switch (reader.Name)
                            {
                                case "node":
                                    nodeType = NODE;
                                    ulong id = Convert.ToUInt64(reader.GetAttribute("id"));
                                    float lat = Convert.ToSingle(reader.GetAttribute("lat"));
                                    float lon = Convert.ToSingle(reader.GetAttribute("lon"));
                                    tempNodes.Add(id, new Graph.GraphNode(id, lat, lon));
                                    break;
                                case "way":
                                    currentWay = new Way(++wayCount);
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
                                    ulong id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                    currentWay.nodes.Add(tempNodes[id]);
                                    break;
                                case "tag":
                                    string key = reader.GetAttribute("k");
                                    if(copykeys.Contains(key))
                                        currentWay.tags.Add(key, reader.GetAttribute("v").ToString());
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
            foreach (Graph.GraphNode node in nodes.Values)
                graph.AddNode(node);
            return graph;
        }
    }
}
