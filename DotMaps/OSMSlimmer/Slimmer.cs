using System;
using DotMaps.Datastructures;
using System.Xml;
using System.Collections.Generic;
using System.Collections;

namespace DotMaps.Utils
{
    class Slimmer
    {

        public Slimmer(string path)
        {
            XmlDocument map = new XmlDocument();
            map.Load(path);
            Graph mapGraph = new Graph();
            List<Address> addressList = new List<Address>();

            this.ReadXMLIntoGraph(map, ref mapGraph, ref addressList);

            Graph.Node[] nodes = new Graph.Node[mapGraph.nodes.Count];
            mapGraph.nodes.Values.CopyTo(nodes, 0);
            foreach (Address address in addressList)
                this.CalculateAssosciatedNode(address, nodes);

            string newPath = path.Substring(0, path.Length - 4) + "_slim.osm";
            WriteToFile(mapGraph, addressList.ToArray(), newPath);
        }
        public void CalculateAssosciatedNode(Address address, Graph.Node[] nodes)
        {
            float shortestDistance = float.MaxValue;
            Graph.Node shortestNode = null;


            foreach (Graph.Node testNode in nodes)
            {
                float testDistance = CalculateDistanceBetweenCoordinates(address.lat, address.lon, testNode.lat, testNode.lon);
                if (testDistance < shortestDistance)
                {
                    shortestDistance = testDistance;
                    shortestNode = testNode;
                }
            }

            address.assosciatedNode = shortestNode.id;
        }

        private Graph ReadXMLIntoGraph(XmlDocument xml, ref Graph graph, ref List<Address> addresslist)
        {
            XmlNode rootNode = xml.ChildNodes[1];
            foreach (XmlNode xmlnode in rootNode.ChildNodes)
            {
                switch (xmlnode.Name)
                {
                    case "node":
                        Graph.Node newNode = ImportNode(xmlnode);
                        graph.nodes.Add(newNode.id, newNode);
                        break;
                    case "way":
                        AddConnections(xmlnode, ref graph, ref addresslist);
                        break;
                    default:
                        Console.WriteLine("Not needed type: {0}", xmlnode.Name);
                        break;
                }
            }

            RemoveNodesWithoutConnection(ref graph);
            return graph;
        }

        private static Graph.Node ImportNode(XmlNode node)
        {
            UInt32 nodeId = Convert.ToUInt32(node.Attributes.GetNamedItem("id").Value);
            float lat = Convert.ToSingle(node.Attributes.GetNamedItem("lat").Value.Replace('.', ','));
            float lon = Convert.ToSingle(node.Attributes.GetNamedItem("lon").Value.Replace('.', ','));
            return new Graph.Node(nodeId, lat, lon);
        }

        private static void AddConnections(XmlNode way, ref Graph graph, ref List<Address> addresslist)
        {
            List<UInt64> wayNodeIds = new List<UInt64>();
            Hashtable tags = new Hashtable();
            Hashtable speeds = new Hashtable();
            foreach (string speedString in System.IO.File.ReadAllLines("speeds.txt"))
                speeds.Add(speedString.Split(',')[0], Convert.ToSingle(speedString.Split(',')[1]));

            foreach (XmlNode attribute in way.ChildNodes)
            {
                if (attribute.Name == "nd")
                    wayNodeIds.Add(Convert.ToUInt64(attribute.Attributes.GetNamedItem("ref").Value));
                else if (attribute.Name == "tag")
                    tags.Add(attribute.Attributes.GetNamedItem("k").Value, attribute.Attributes.GetNamedItem("v").Value);
            }
            UInt64[] nodeIds = wayNodeIds.ToArray();

            if (tags.ContainsKey("building") && tags.ContainsKey("addr:country") && tags.ContainsKey("addr:postcode") && tags.ContainsKey("addr:street"))
            {
                string county = (string)tags["addr:country"];
                UInt16 postcode = Convert.ToUInt16((string)tags["addr:postcode"]);
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
                            Console.WriteLine("Warn: unexpected value for maxspeed: {0}", (string)tags["maxspeed"]);
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
                    //TODO
                }
            }
            else
                return;
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

        private static void ConvertAddressesToXML(ref XmlDocument xml, ref XmlNode addressRoot, Address[] addresses)
        {
            foreach (Address address in addresses)
            {
                XmlNode country = null;
                foreach (XmlNode node in xml.GetElementsByTagName("country"))
                    if (node.Attributes.GetNamedItem("code").Value == address.country)
                        country = node;
                if (country == null)
                {
                    country = xml.CreateElement("country");
                    addressRoot.AppendChild(country);
                    XmlAttribute code = xml.CreateAttribute("code");
                    code.Value = address.country;
                    country.Attributes.Append(code);
                }

                XmlNode city = null;
                foreach (XmlNode node in xml.GetElementsByTagName("city"))
                    if (node.Attributes.GetNamedItem("postcode").Value == address.postcode.ToString())
                        city = node;
                if (city == null)
                {
                    city = xml.CreateElement("city");
                    country.AppendChild(city);
                    XmlAttribute name = xml.CreateAttribute("name");
                    name.Value = address.cityname;
                    city.Attributes.Append(name);
                    XmlAttribute postcode = xml.CreateAttribute("postcode");
                    postcode.Value = address.postcode.ToString();
                    city.Attributes.Append(postcode);
                }

                XmlNode street = null;
                foreach (XmlNode node in xml.GetElementsByTagName("street"))
                    if (node.Attributes.GetNamedItem("name").Value == address.steetname)
                        street = node;
                if (street == null)
                {
                    street = xml.CreateElement("street");
                    city.AppendChild(street);
                    XmlAttribute name = xml.CreateAttribute("name");
                    name.Value = address.steetname;
                    street.Attributes.Append(name);
                }

                XmlNode housenumber = xml.CreateElement("house");

                XmlAttribute number = xml.CreateAttribute("number");
                number.Value = address.housenumber.ToString();
                housenumber.Attributes.Append(number);
                XmlAttribute lat = xml.CreateAttribute("lat");
                lat.Value = address.lat.ToString();
                housenumber.Attributes.Append(lat);
                XmlAttribute lon = xml.CreateAttribute("lon");
                lon.Value = address.lon.ToString();
                housenumber.Attributes.Append(lon);
                XmlAttribute nodeId = xml.CreateAttribute("node");
                nodeId.Value = address.assosciatedNode.ToString();
                housenumber.Attributes.Append(nodeId);

                street.AppendChild(housenumber);
            }
        }

        private static void ConvertGraphToXML(ref XmlDocument xml, ref XmlNode nodeRoot, Graph graph)
        {

            foreach (Graph.Node graphNode in graph.nodes.Values)
            {
                XmlNode newXmlNode = xml.CreateElement("node");

                XmlAttribute lat = xml.CreateAttribute("lat");
                lat.Value = graphNode.lat.ToString();
                newXmlNode.Attributes.Append(lat);
                XmlAttribute lon = xml.CreateAttribute("lon");
                lon.Value = graphNode.lon.ToString();
                newXmlNode.Attributes.Append(lon);
                XmlAttribute id = xml.CreateAttribute("id");
                id.Value = graphNode.id.ToString();
                newXmlNode.Attributes.Append(id);

                XmlNode connections = xml.CreateElement("connections");
                newXmlNode.AppendChild(connections);
                foreach (Graph.Connection connection in graphNode.GetConnections())
                {
                    XmlNode newXmlConnection = xml.CreateElement("connection");

                    XmlAttribute connectionNodeId = xml.CreateAttribute("id");
                    connectionNodeId.Value = connection.neighbor.id.ToString();
                    newXmlConnection.Attributes.Append(connectionNodeId);
                    XmlAttribute distance = xml.CreateAttribute("distance");
                    distance.Value = connection.distance.ToString();
                    newXmlConnection.Attributes.Append(distance);
                    XmlAttribute timeNeeded = xml.CreateAttribute("time");
                    timeNeeded.Value = connection.timeNeeded.ToString();
                    newXmlConnection.Attributes.Append(timeNeeded);
                    XmlAttribute type = xml.CreateAttribute("type");
                    type.Value = connection.type;
                    newXmlConnection.Attributes.Append(type);

                    connections.AppendChild(newXmlConnection);
                }

                nodeRoot.AppendChild(newXmlNode);
            }
        }
        private static void WriteToFile(Graph mapGraph, Address[] addressList, string path)
        {
            XmlDocument slimmed = new XmlDocument();
            XmlNode root = slimmed.CreateElement("slim");
            slimmed.AppendChild(root);
            XmlNode nodes = slimmed.CreateElement("nodes");
            root.AppendChild(nodes);
            XmlNode addresses = slimmed.CreateElement("addresses");
            root.AppendChild(addresses);

            ConvertGraphToXML(ref slimmed, ref nodes, mapGraph);
            ConvertAddressesToXML(ref slimmed, ref addresses, addressList);
            slimmed.Save(path);
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
