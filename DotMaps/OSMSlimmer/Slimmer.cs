using DotMaps.Utils;
using DotMaps.Datastructures;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DotMaps
{
    public class Slimmer
    {
        public static void Slim(string path)
        {
            Hashtable nodes = new Hashtable();
            List<Way> ways = new List<Way>();
            string bounds = Reader.ReadOSMXml(path, ref nodes, ref ways);

            Hashtable neededNodes = new Hashtable();
            foreach (Way way in ways)
                if (way.tags.ContainsKey("highway"))
                {
                    foreach (ulong nodeId in way.nodes)
                        if (!neededNodes.ContainsKey(nodeId))
                            neededNodes.Add(nodeId, nodes[nodeId]);
                }
                else if (way.tags.ContainsKey("addr:housenumber"))
                {
                    if (!neededNodes.ContainsKey(way.nodes[0]))
                        neededNodes.Add(way.nodes[0], nodes[way.nodes[0]]);
                    way.nodes.RemoveRange(1, way.nodes.Count - 1);
                }


            string newPath = path.Substring(0, path.Length - 4) + "_slim.osm";
            Writer.WriteOSMXml(newPath, bounds, neededNodes, ways);
        } 

        public static List<Address> CalculateAddresses(string path)
        {
            Hashtable nodes = new Hashtable();
            List<Way> ways = new List<Way>();
            string bounds = Reader.ReadOSMXml(path, ref nodes, ref ways);
            Graph graph = Reader.ReadNodesIntoGraph(nodes);
            List<Address> addresses = new List<Address>();

            foreach (Way way in ways)
            {
                if (way.tags.ContainsKey("addr:country") && way.tags.ContainsKey("addr:postcode") && way.tags.ContainsKey("addr:street"))
                {
                    string county = (string)(way.tags["addr:country"]);
                    string postcode = (string)way.tags["addr:postcode"];
                    string city = way.tags.ContainsKey("addr:city") ? (string)way.tags["addr:city"] : "";
                    string street = (string)way.tags["addr:street"];
                    string housenumber = way.tags.ContainsKey("addr:housenumber") ? (string)way.tags["addr:housenumber"] : "0";

                    Node buildingNode = (Node)graph.nodes[way.nodes[0]];
                    if (buildingNode != null)
                        addresses.Add(new Address(county, postcode, city, street, housenumber, buildingNode.lat, buildingNode.lon));
                    else
                        Console.WriteLine(way.nodes[0]);
                }
            }

            float minLat = Convert.ToSingle(bounds.Substring(bounds.IndexOf("minlat") + 8, bounds.IndexOf('"', bounds.IndexOf("minlat") + 9) - bounds.IndexOf("minlat") - 8).Replace('.', ','));
            float minLon = Convert.ToSingle(bounds.Substring(bounds.IndexOf("minlon") + 8, bounds.IndexOf('"', bounds.IndexOf("minlon") + 9) - bounds.IndexOf("minlon") - 8).Replace('.', ','));
            float maxLat = Convert.ToSingle(bounds.Substring(bounds.IndexOf("maxlat") + 8, bounds.IndexOf('"', bounds.IndexOf("maxlat") + 9) - bounds.IndexOf("maxlat") - 8).Replace('.', ','));
            float maxLon = Convert.ToSingle(bounds.Substring(bounds.IndexOf("maxlon") + 8, bounds.IndexOf('"', bounds.IndexOf("maxlon") + 9) - bounds.IndexOf("maxlon") - 8).Replace('.', ','));

            double latDiff = CalculateDistanceBetweenCoordinates(minLat, minLon, maxLat, minLon);
            double lonDiff = CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, maxLon);

            List<Node>[,] grid = new List<Node>[(int)Math.Ceiling(lonDiff)+1, (int)Math.Ceiling(latDiff)+1];
            for (int px = 0; px < grid.GetLength(0); px++)
                for (int py = 0; py < grid.GetLength(1); py++)
                    grid[px, py] = new List<Node>();

            int nodeX, nodeY;
            foreach (Node node in graph.nodes.Values)
            {
                nodeX = (int)Math.Floor(CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, node.lon));
                nodeY = (int)Math.Floor(CalculateDistanceBetweenCoordinates(minLat, minLon, node.lat, minLon));
                grid[nodeX, nodeY].Add(node);
            }

            float shortestDistance, testDistance;
            Node shortestNode = null;
            int addressX, addressY;
            List<Node> search;
            foreach (Address address in addresses)
            {
                addressX = (int)Math.Ceiling(CalculateDistanceBetweenCoordinates(minLat, minLon, minLat, address.lon));
                addressY = (int)Math.Ceiling(CalculateDistanceBetweenCoordinates(minLat, minLon, address.lat, minLon));

                shortestDistance = float.MaxValue;

                search = new List<Node>();
                for (int px = (addressX > 0) ? addressX - 1 : 0; px < grid.GetLength(0); px++)
                    for (int py = (addressY > 0) ? addressY - 1 : 0; py < grid.GetLength(1); py++)
                        search.AddRange(grid[px, py]);

                foreach (Node testNode in search)
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

            return addresses;
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

        private static float DegreesToRadians(float degrees)
        {
            return degrees * Convert.ToSingle(Math.PI) / 180;
        }

        public static void Main(string[] args)
        {
            Slimmer.Slim(@"C:\Users\Jann\Desktop\ersdorf.osm");
        }
    }
}
