using System;
using System.Threading.Tasks;
using DotMaps.Datastructures;
using System.Xml;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace DotMaps.Utils
{
    class Slimmer
    {

        public Slimmer(string path)
        {
            List<Address> addressList = new List<Address>();

            Console.WriteLine("Importing Nodes and Connections...");
            Graph mapGraph = ConvertXMLtoGraph(path, addressList);
            Console.WriteLine("Done. {0} Nodes", mapGraph.nodes.Count);

            Console.WriteLine("Removing unnecessary Nodes...");
            RemoveNodesWithoutConnection(ref mapGraph);
            Console.WriteLine("Done. {0} Nodes", mapGraph.nodes.Count);

            Console.WriteLine("Importing Addresses (This is the longest part)...");
            this.CalculateAssosciatedNodes(addressList, mapGraph);
            Console.WriteLine("Done.");

            Console.WriteLine("Writing new file.");
            string newPath = path.Substring(0, path.Length - 4) + "_slim.osm";
            WriteToFile(mapGraph, addressList.ToArray(), newPath);
        }

        public Graph ConvertXMLtoGraph(string path, List<Address> addresslist)
        {
            Graph graph = new Graph();

            Hashtable speeds = new Hashtable();
            foreach (string speedString in File.ReadAllLines("speeds.txt"))
                speeds.Add(speedString.Split(',')[0], Convert.ToSingle(speedString.Split(',')[1]));

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true
            };
            XmlReader reader = XmlReader.Create(path, settings);

            string parent = "";
            const byte READING = 1, DONE = 0;
            byte state = DONE;
            List<UInt64> wayNodeIds = new List<UInt64>();
            Hashtable tags = new Hashtable();

            while (reader.Read())
            {
                if (reader.Depth == 1)
                {
                    if (state == READING)
                    {
                        state = DONE;
                        UInt64[] nodeIds = wayNodeIds.ToArray();

                        if (tags.ContainsKey("building") && tags.ContainsKey("addr:country") && tags.ContainsKey("addr:postcode") && tags.ContainsKey("addr:street"))
                        {
                            string county = (string)tags["addr:country"];
                            UInt16 postcode = 0;
                            try
                            {
                                postcode = Convert.ToUInt16((string)tags["addr:postcode"]);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Format Exception postcode: {0}", (string)tags["addr:postcode"]);
                            }
                            string city = tags.ContainsKey("addr:city") ? (string)tags["addr:city"] : "";
                            string street = (string)tags["addr:street"];
                            string housenumber = tags.ContainsKey("addr:housenumber") ? (string)tags["addr:housenumber"] : "0";

                            Graph.Node buildingNode = (Graph.Node)graph.nodes[nodeIds[0]];

                            Address newAddress = new Address(county, postcode, city, street, housenumber, buildingNode.lat, buildingNode.lon);
                            addresslist.Add(newAddress);
                        }
                        else if (tags.ContainsKey("highway"))
                        {
                            for (int i = 1; i < nodeIds.Length; i++)
                            {
                                Graph.Node destination = (Graph.Node)graph.nodes[nodeIds[i]];
                                Graph.Node start = (Graph.Node)graph.nodes[nodeIds[i - 1]];

                                float distance = CalculateDistanceBetween(destination, start);
                                int speed = speeds.ContainsKey(tags["highway"].ToString()) ? Convert.ToInt32(speeds[tags["highway"].ToString()]) : 1;
                                if (tags.ContainsKey("maxspeed"))
                                {
                                    try
                                    {
                                        speed = tags.ContainsKey("maxspeed") ? Convert.ToInt32((string)tags["maxspeed"]) : speed;
                                    }
                                    catch (FormatException)
                                    {
                                        switch ((string)tags["maxspeed"])
                                        {
                                            case "none":
                                                speed = 150;
                                                break;
                                            default:
                                                Console.WriteLine("Warn: unexpected value for maxspeed: {0}", (string)tags["maxspeed"]);
                                                break;
                                        }
                                    }
                                }
                                float timeNeeded = distance / speed;
                                string type = (string)tags["highway"];

                                Graph.Connection to = new Graph.Connection(distance, timeNeeded, start, type);
                                destination.AddConnection(to);
                                if (!tags.ContainsKey("oneway") || (string)tags["oneway"] != "yes")
                                {
                                    Graph.Connection from = new Graph.Connection(distance, timeNeeded, destination, type);
                                    start.AddConnection(from);
                                }
                            }

                            if (tags.ContainsKey("name"))
                            {
                                //TODO streetname
                            }
                        }
                    }
                    parent = reader.Name;
                    if (reader.Name == "node" && reader.NodeType != XmlNodeType.EndElement)
                    {
                        UInt64 nodeId = Convert.ToUInt64(reader.GetAttribute("id"));
                        float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace('.', ','));
                        float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace('.', ','));
                        Graph.Node newNode = new Graph.Node(nodeId, lat, lon);
                        graph.nodes.Add(nodeId, newNode);
                    }
                }
                else if (reader.Depth == 2 && parent == "way")
                {
                    if (state == DONE)
                    {
                        wayNodeIds = new List<UInt64>();
                        tags = new Hashtable();
                        state = READING;
                    }
                    if (reader.Name == "nd")
                        wayNodeIds.Add(Convert.ToUInt64(reader.GetAttribute("ref")));
                    else if (reader.Name == "tag")
                        tags.Add(reader.GetAttribute("k"), reader.GetAttribute("v"));
                }
            }
            reader.Close();

            return graph;
        }

        public void CalculateAssosciatedNodes(List<Address> addresses, Graph graph)
        {
            List<Graph.Node>[,] grid = this.CreateGrid(graph);

            float minLat = float.MaxValue, minLon = float.MaxValue, maxLat = float.MinValue, maxLon = float.MinValue;
            foreach (Graph.Node node in graph.nodes.Values)
            {
                if (minLat > node.lat)
                    minLat = node.lat;
                if (minLon > node.lon)
                    minLon = node.lon;
                if (maxLat < node.lat)
                    maxLat = node.lat;
                if (maxLon < node.lon)
                    maxLon = node.lon;
            }

            int saveLeft = Console.CursorLeft;
            int saveTop = Console.CursorTop;
            uint count = 0;
            DateTime start = DateTime.Now;
            TimeSpan elapsed;

            float shortestDistance, testDistance;
            Graph.Node shortestNode = null;
            int x, y;
            List<Graph.Node> search;
            foreach (Address address in addresses)
            {
                Console.SetCursorPosition(saveLeft, saveTop);
                elapsed = DateTime.Now.Subtract(start);
                Console.WriteLine("Calculating {0}/{1} {2}s", ++count, addresses.Count, elapsed.TotalSeconds / count * (addresses.Count - count));
                x = (int)Math.Ceiling(CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, address.lon));
                y = (int)Math.Ceiling(CalculateDistanceBetweenCoordinates(minLat, minLon, address.lat, minLon));

                shortestDistance = float.MaxValue;

                search = new List<Graph.Node>();
                for(int px = (x > 0) ? x-1 : 0 ; px < grid.GetLength(0) ; px++)
                    for (int py = (y > 0) ? y - 1 : 0; py < grid.GetLength(1); py++)
                        search.AddRange(grid[px, py]);

                foreach (Graph.Node testNode in search)
                {
                    testDistance = CalculateDistanceBetweenCoordinates(address.lat, address.lon, testNode.lat, testNode.lon);
                    if (testDistance < shortestDistance)
                    {
                        shortestDistance = testDistance;
                        shortestNode = testNode;
                    }
                }

                address.assosciatedNode = shortestNode.id;
            }
        }
        private List<Graph.Node>[,] CreateGrid(Graph graph)
        {
            float minLat = float.MaxValue, minLon = float.MaxValue, maxLat = float.MinValue, maxLon = float.MinValue;
            foreach (Graph.Node node in graph.nodes.Values)
            {
                if (minLat > node.lat)
                    minLat = node.lat;
                if (minLon > node.lon)
                    minLon = node.lon;
                if (maxLat < node.lat)
                    maxLat = node.lat;
                if (maxLon < node.lon)
                    maxLon = node.lon;
            }

            double latDiff = CalculateDistanceBetweenCoordinates(minLat, minLon, maxLat, minLon);
            double lonDiff = CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, maxLon);
            Console.WriteLine("Size of area: {0}x{1}km", (int)Math.Ceiling(lonDiff), (int)Math.Ceiling(latDiff));

            List<Graph.Node>[,] grid = new List<Graph.Node>[(int)Math.Ceiling(lonDiff), (int)Math.Ceiling(latDiff)];
            for (int px = 0; px < grid.GetLength(0); px++)
                for (int py = 0; py < grid.GetLength(1); py++)
                    grid[px, py] = new List<Graph.Node>();

            foreach (Graph.Node node in graph.nodes.Values)
            {
                int x = (int)Math.Floor(CalculateDistanceBetweenCoordinates(node.lat, minLon, node.lat, node.lon));
                int y = (int)Math.Floor(CalculateDistanceBetweenCoordinates(minLat, node.lon, node.lat, node.lon));
                grid[x, y].Add(node);
            }
            return grid;
        }

        private static float CalculateDistanceBetweenCoordinates(float lat1, float lon1, float lat2, float lon2)
        {
            int earthRadius = 6371;
            float differenceLat = DegreesToRadians(lat2 - lat1);
            float differenceLon = DegreesToRadians(lon2 - lon1);

            float lat1Rads = DegreesToRadians(lat1);
            float lat2Rads = DegreesToRadians(lat2);

            double a = Math.Sin(differenceLat / 2) * Math.Sin(differenceLat / 2) + Math.Sin(differenceLon / 2) * Math.Sin(differenceLon / 2) * Math.Cos(lat1Rads) * Math.Cos(lat2Rads);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Convert.ToSingle(earthRadius * c);
        }

        private static float CalculateDistanceBetween(Graph.Node node1, Graph.Node node2)
        {
            return CalculateDistanceBetweenCoordinates(node1.lat, node1.lon, node2.lat, node2.lon);
        }

        private static float DegreesToRadians(float degrees)
        {
            return degrees * Convert.ToSingle(Math.PI) / 180;
        }

        private static void RemoveNodesWithoutConnection(ref Graph graph)
        {
            Graph.Node[] nodes = new Graph.Node[graph.nodes.Count];
            graph.nodes.Values.CopyTo(nodes, 0);


            foreach (Graph.Node node in nodes)
                foreach (Graph.Connection connection in node.GetConnections())
                {
                    node.timeRequired = double.MinValue;
                    connection.neighbor.timeRequired = double.MinValue;
                }

            foreach (Graph.Node node in nodes)
                if (node.timeRequired == double.MinValue)
                    node.timeRequired = double.MaxValue;
                else
                    graph.nodes.Remove(node.id);
        }

        private static void WriteToFile(Graph mapGraph, Address[] addressList, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<slim>");
                writer.WriteLine("  <nodes>");
                foreach (Graph.Node node in mapGraph.nodes.Values)
                {
                    writer.WriteLine("    <node lat=\"" + node.lat.ToString() + "\" lon=\"" + node.lon.ToString() + "\" id=\"" + node.id + "\">");
                    if (node.GetConnections().Length < 1)
                        writer.WriteLine("      <connections />");
                    else
                    {
                        writer.WriteLine("      <connections>");
                        foreach (Graph.Connection connection in node.GetConnections())
                        {
                            writer.WriteLine("        <connection id=\"" + connection.neighbor.id + "\" distance=\"" + connection.distance + "\" time=\"" + connection.distance + "\" type=\"" + connection.type + "\" />");
                        }
                        writer.WriteLine("      </connections>");
                    }
                    writer.WriteLine("    </node>");
                }
                writer.WriteLine("  </nodes>");
                writer.WriteLine("  <addresses>");
                foreach (Address address in addressList)
                {
                    writer.WriteLine("    <address id=\""+address.assosciatedNode.ToString()+"\" countrycode=\"" + address.country + "\" cityname=\"" + address.cityname + "\" postcode=\"" + address.postcode.ToString() + "\" streetname=\"" + address.steetname + "\" housenumber=\"" + address.housenumber + "\" lat=\"" + address.lat + "\" lon=\"" + address.lon + "\" />");
                }
                writer.WriteLine("  </addresses>");
                writer.WriteLine("</slim>");
                writer.Close();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
                new Slimmer(args[0]);
            else
            {
                Console.WriteLine("What is the path to the .osm file?");
                new Slimmer(@Console.ReadLine());
            }

        }
    }
}
