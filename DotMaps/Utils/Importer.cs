using System;
using System.Collections.Generic;
using System.Collections;
using DotMaps.Datastructures;

namespace DotMaps.Utils
{
    public class Importer
    {
        public static Graph ReadNodesIntoNewGraph(Hashtable nodes)
        {
            Graph map = new Graph();
            return ReadNodesIntoGraph(nodes, map);
        }

        public static Graph ReadNodesIntoGraph(Hashtable nodes, Graph graph)
        {
            foreach (Node node in nodes.Values)
                graph.nodes.Add(node.id, node);
            return graph;
        }

        public static void ReadWaysIntoGraph(List<Way> ways, ref Graph graph)
        {
            Hashtable speeds = new Hashtable();
            foreach (string speed in System.IO.File.ReadAllLines("speeds.txt"))
                speeds.Add(speed.Split(',')[0], Convert.ToInt32(speed.Split(',')[1]));

            foreach (Way way in ways)
            {
                UInt64[] nodeIds = way.nodes.ToArray();
                for (int i = 1; i < nodeIds.Length; i++)
                {
                    Node fromNode = (Node)graph.nodes[nodeIds[i]];
                    Node toNode = (Node)graph.nodes[nodeIds[i - 1]];

                    double distance = Functions.CalculateDistanceBetweenCoordinates(fromNode.lat, fromNode.lon, toNode.lat, toNode.lon);
                    int speed = 50;
                    if (way.tags.ContainsKey("highway") && speeds.ContainsKey((string)way.tags["highway"]))
                        speed = (int)speeds[(string)way.tags["highway"]];
                    if (way.tags.ContainsKey("maxspeed"))
                        try
                        {
                            speed = Convert.ToInt32((string)way.tags["maxspeed"]);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Maxspeed {0} not implemented", (string)way.tags["maxspeed"]);
                        }
                    float timeNeeded = (float)distance / speed;

                    string name = "";
                    if (way.tags.ContainsKey("ref"))
                        name = (string)way.tags["ref"];
                    else if (way.tags.ContainsKey("name"))
                        name = (string)way.tags["name"];

                    toNode.AddConnection(new Connection(distance, timeNeeded, fromNode, (string)way.tags["highway"], name));
                    if (!way.tags.ContainsKey("oneway") || ((string)way.tags["oneway"]) == "no")
                        fromNode.AddConnection(new Connection(distance, timeNeeded, toNode, (string)way.tags["highway"], name));
                }
            }
        }
    }
}
