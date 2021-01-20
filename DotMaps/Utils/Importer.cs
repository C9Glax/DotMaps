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

        /*
         * Event Handler
         */
        public event StatusEventHandler OnStatusChange;
        public class StatusChangedEventArgs : EventArgs
        {
            public string status;
        }
        public delegate void StatusEventHandler(object sender, StatusChangedEventArgs e);

        public event ProgressEventHandler OnProgress;
        public class ProgressEventArgs : EventArgs
        {
            public int progress;
        }
        public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

        private enum NodeType { UNKNOWN, NODE, WAY }
        public Graph ImportOSM(string path)
        {
            Graph retGraph = new Graph();

            this.OnProgress?.Invoke(this, new ProgressEventArgs
            {
                progress = 0
            });
            this.OnStatusChange?.Invoke(this, new StatusChangedEventArgs
            {
                status = "Importing Nodes and Counting Occurances..."
            });

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

            List<string> copykeys = new List<string>();
            foreach (string key in File.ReadAllLines("copykeys.txt"))
                copykeys.Add(key);

            List<Graph.GraphNode> nodes = new List<Graph.GraphNode>();
            Dictionary<string, string> tags = new Dictionary<string, string>();


            this.OnStatusChange?.Invoke(this, new StatusChangedEventArgs
            {
                status = "Splitting and Importing Ways"
            });
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
                            if (tags.ContainsKey("highway") && nodes.Count > 1)
                            {
                                int speed = (int)speeds["default"];
                                if (tags.ContainsKey("maxspeed"))
                                    try
                                    {
                                        speed = Convert.ToInt32(tags["maxspeed"]);
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Maxspeed '{0}' not implemented", tags["maxspeed"]);
                                    }
                                else if (speeds.ContainsKey(tags["highway"]))
                                    speed = (int)speeds[tags["highway"]];
                                string name = "";
                                if (tags.ContainsKey("ref"))
                                    name = tags["ref"];
                                else if (tags.ContainsKey("name"))
                                    name = tags["name"]; ;

                                Graph.GraphNode start = retGraph.GetNode(nodes[0].id);
                                List<_3DNode> coords = new List<_3DNode>();
                                double distance = 0.0;

                                for (int i = 1; i < nodes.Count - 1; i++)
                                {
                                    if (nodeOccurances[nodes[i].id] > 1)
                                    {
                                        Graph.GraphNode intersection = retGraph.GetNode(nodes[i].id);
                                        if (!tags.ContainsKey("oneway") || tags["oneway"] == "no")
                                            start.connections.Add(new Graph.Connection(distance, (float)distance / speed, intersection, name, coords.ToArray(), tags["highway"]));
                                        coords.Reverse();
                                        intersection.connections.Add(new Graph.Connection(distance, (float)distance / speed, start, name, coords.ToArray(), tags["highway"]));

                                        start = intersection;
                                        distance = 0;
                                        coords.Clear();
                                    }
                                    else
                                    {
                                        float lat = nodes[i].coordinates.lat;
                                        float lon = nodes[i].coordinates.lon;
                                        distance += coords.Count > 0 ? Functions.DistanceBetweenCoordinates(coords[coords.Count - 1].lat, coords[coords.Count - 1].lon, lat, lon) : 0;
                                        coords.Add(new _3DNode(lat, lon));
                                        retGraph.RemoveNode(nodes[i].id);
                                    }
                                }

                                Graph.GraphNode goal = retGraph.GetNode(nodes[nodes.Count - 1].id);
                                if (!tags.ContainsKey("oneway") || tags["oneway"] == "no")
                                    start.connections.Add(new Graph.Connection(distance, (float)distance / speed, goal, name, coords.ToArray(), tags["highway"]));
                                coords.Reverse();
                                goal.connections.Add(new Graph.Connection(distance, (float)distance / speed, start, name, coords.ToArray(), tags["highway"]));
                            }
                        }
                        switch (reader.Name)
                        {
                            case "node":
                                nodeType = NodeType.NODE;
                                break;
                            case "way":
                                nodeType = NodeType.WAY;
                                nodes.Clear();
                                tags.Clear();
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
                                    nodes.Add(retGraph.GetNode(id));
                                else if (allNodes.ContainsKey(id))
                                {
                                    retGraph.AddNode((Graph.GraphNode)allNodes[id]);
                                    allNodes.Remove(id);
                                    nodes.Add(retGraph.GetNode(id));
                                }
                                break;
                            case "tag":
                                if(copykeys.Contains(reader.GetAttribute("k")))
                                    tags.Add(reader.GetAttribute("k"), reader.GetAttribute("v").ToString());
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
