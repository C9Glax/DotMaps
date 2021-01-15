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
        private enum NodeType { UNKNOWN, NODE, WAY }
        public static Graph ImportOSM(string path)
        {
            Graph retGraph = new Graph();

            Dictionary<ulong, uint> nodeOccurances = new Dictionary<ulong, uint>();
            Hashtable allNodes = new Hashtable();
            XmlReaderSettings readerSettings = new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            };
            using (XmlReader reader = XmlReader.Create(path, readerSettings))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        switch (reader.Name)
                        {
                            case "node":
                                ulong id = Convert.ToUInt64(reader.GetAttribute("id"));
                                float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace(".", ","));
                                float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace(".", ","));
                                allNodes.Add(id, new Graph.GraphNode(id, lat, lon));
                                break;
                            case "bounds":
                                retGraph.minLat = Convert.ToSingle(reader.GetAttribute("minlat").Replace(".", ","));
                                retGraph.minLon = Convert.ToSingle(reader.GetAttribute("minlon").Replace(".", ","));
                                retGraph.maxLat = Convert.ToSingle(reader.GetAttribute("maxlat").Replace(".", ","));
                                retGraph.maxLon = Convert.ToSingle(reader.GetAttribute("maxlon").Replace(".", ","));
                                break;
                            case "nd":
                                ulong nodeID = Convert.ToUInt64(reader.GetAttribute("ref"));
                                if (!nodeOccurances.ContainsKey(nodeID))
                                    nodeOccurances.Add(nodeID, 1);
                                else
                                    nodeOccurances[nodeID]++;
                                break;
                        }
                    }
                }
                reader.Close();
            }

            Hashtable speeds = new Hashtable();
            foreach (string speed in File.ReadAllLines("speeds.txt"))
                speeds.Add(speed.Split(',')[0], Convert.ToInt32(speed.Split(',')[1]));

            Way currentWay = new Way(0);

            
            NodeType nodeType = NodeType.UNKNOWN;
            using (XmlReader reader = XmlReader.Create(path, readerSettings))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.Depth == 1)
                    {
                        if (nodeType == NodeType.WAY)
                        {
                            if (currentWay.tags.ContainsKey("highway") && currentWay.nodes.Count > 1)
                            {
                                int speed = (int)speeds["default"];
                                if (currentWay.tags.ContainsKey("maxspeed"))
                                    try
                                    {
                                        speed = Convert.ToInt32((string)currentWay.tags["maxspeed"]);
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Maxspeed {0} not implemented", (string)currentWay.tags["maxspeed"]);
                                    }
                                else if (speeds.ContainsKey((string)currentWay.tags["highway"]))
                                    speed = (int)speeds[(string)currentWay.tags["highway"]];
                                string name = "";
                                if (currentWay.tags.ContainsKey("ref"))
                                    name = currentWay.tags["ref"];
                                else if (currentWay.tags.ContainsKey("name"))
                                    name = currentWay.tags["name"]; ;

                                Graph.GraphNode start = retGraph.GetNode(currentWay.nodes[0].id);
                                List<_3DNode> coords = new List<_3DNode>();
                                double distance = 0.0;

                                for (int i = 1; i < currentWay.nodes.Count - 1; i++)
                                {
                                    if (nodeOccurances[currentWay.nodes[i].id] > 1)
                                    {
                                        Graph.GraphNode intersection = retGraph.GetNode(currentWay.nodes[i].id);
                                        if (!currentWay.tags.ContainsKey("oneway") || ((string)currentWay.tags["oneway"]) == "no")
                                            start.AddConnection(new Graph.Connection(distance, (float)distance / speed, intersection, name, coords.ToArray()));
                                        coords.Reverse();
                                        intersection.AddConnection(new Graph.Connection(distance, (float)distance / speed, start, name, coords.ToArray()));

                                        start = intersection;
                                        distance = 0;
                                        coords.Clear();
                                    }
                                    else
                                    {
                                        float lat = currentWay.nodes[i].lat;
                                        float lon = currentWay.nodes[i].lon;
                                        distance += coords.Count > 0 ? Functions.DistanceBetweenCoordinates(coords[coords.Count - 1].lat, coords[coords.Count - 1].lon, lat, lon) : 0;
                                        coords.Add(new _3DNode(lat, lon));
                                    }
                                }

                                Graph.GraphNode goal = retGraph.GetNode(currentWay.nodes[currentWay.nodes.Count - 1].id);
                                if (!currentWay.tags.ContainsKey("oneway") || ((string)currentWay.tags["oneway"]) == "no")
                                    start.AddConnection(new Graph.Connection(distance, (float)distance / speed, goal, name, coords.ToArray()));
                                coords.Reverse();
                                goal.AddConnection(new Graph.Connection(distance, (float)distance / speed, start, name, coords.ToArray()));
                            }
                        }
                        switch (reader.Name)
                        {
                            case "node":
                                nodeType = NodeType.NODE;
                                break;
                            case "way":
                                nodeType = NodeType.WAY;
                                currentWay.nodes.Clear();
                                currentWay.tags.Clear();
                                currentWay.id = Convert.ToUInt64(reader.GetAttribute("id"));
                                break;
                            default:
                                nodeType = NodeType.UNKNOWN;
                                break;
                        }
                    }
                    else if (reader.Depth == 2 && nodeType == NodeType.WAY)
                    {
                        switch (reader.Name)
                        {
                            case "nd":
                                ulong id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                if (retGraph.ContainsNode(id))
                                    currentWay.nodes.Add(retGraph.GetNode(id));
                                else if (allNodes.ContainsKey(id))
                                {
                                    retGraph.AddNode((Graph.GraphNode)allNodes[id]);
                                    allNodes.Remove(id);
                                    currentWay.nodes.Add(retGraph.GetNode(id));
                                }
                                break;
                            case "tag":
                                currentWay.tags.Add(reader.GetAttribute("k"), reader.GetAttribute("v").ToString());
                                break;
                        }
                    }
                }
                reader.Close();
            }
            
            return retGraph;
        }
    }
}
