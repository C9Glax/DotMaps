using System;
using System.Xml;
using DotMaps.Datastructures;

namespace DotMaps.Utils
{
    public class Loader
    {

        public static Graph LoadXMLGraph(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@path);
            XmlNode root = xmlDoc.GetElementsByTagName("nodes")[0];

            Graph graph = new Graph();

            foreach (XmlNode xmlNode in root.ChildNodes)
            {
                UInt32 nodeId = Convert.ToUInt32(xmlNode.Attributes.GetNamedItem("id").Value);
                float lat = Convert.ToSingle(xmlNode.Attributes.GetNamedItem("lat").Value);
                float lon = Convert.ToSingle(xmlNode.Attributes.GetNamedItem("lon").Value);
                Graph.Node newNode = new Graph.Node(nodeId, lat, lon);
                graph.nodes.Add(nodeId, newNode);
            }

            foreach (XmlNode xmlNode in root.ChildNodes)
            {
                UInt64 nodeId = Convert.ToUInt64(xmlNode.Attributes.GetNamedItem("id").Value);
                Graph.Node currentNode = (Graph.Node)graph.nodes[nodeId];
                foreach (XmlNode xmlConnection in xmlNode.FirstChild.ChildNodes)
                {
                    double distance = Convert.ToDouble(xmlConnection.Attributes.GetNamedItem("distance").Value);
                    float time = Convert.ToSingle(xmlConnection.Attributes.GetNamedItem("time").Value);
                    string type = xmlConnection.Attributes.GetNamedItem("type").Value;

                    UInt64 id = Convert.ToUInt64(xmlConnection.Attributes.GetNamedItem("id").Value);
                    Graph.Node neighbor = (Graph.Node)graph.nodes[id];

                    Graph.Connection newConnection = new Graph.Connection(distance, time, neighbor, type);

                    currentNode.AddConnection(newConnection);
                }
            }

            return graph;
        }

        public static Address[] LoadXMLAddresses(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@path);
            XmlNode root = xmlDoc.GetElementsByTagName("addresses")[0];

            System.Collections.Generic.List<Address> addresses = new System.Collections.Generic.List<Address>();

            foreach (XmlNode country in root.ChildNodes)
            {
                string countrycode = country.Attributes.GetNamedItem("code").Value;
                foreach (XmlNode city in country.ChildNodes)
                {
                    string cityname = city.Attributes.GetNamedItem("name").Value;
                    ushort postcode = Convert.ToUInt16(city.Attributes.GetNamedItem("postcode").Value);
                    foreach (XmlNode street in city.ChildNodes)
                    {
                        string streetname = street.Attributes.GetNamedItem("name").Value;
                        foreach (XmlNode house in street.ChildNodes)
                        {
                            string housenumber = house.Attributes.GetNamedItem("number").Value;
                            float lat = Convert.ToSingle(house.Attributes.GetNamedItem("lat").Value);
                            float lon = Convert.ToSingle(house.Attributes.GetNamedItem("lon").Value);
                            UInt32 node = Convert.ToUInt32(house.Attributes.GetNamedItem("node").Value);

                            Address newAddress = new Address(countrycode, postcode, cityname, streetname, housenumber, lat, lon, node);
                            addresses.Add(newAddress);
                        }
                    }
                }
            }

            return addresses.ToArray();
        }
    }
}
