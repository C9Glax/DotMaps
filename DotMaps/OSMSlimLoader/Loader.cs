using System;
using System.Collections.Generic;
using System.Xml;
using DotMaps.Datastructures;

namespace DotMaps.Utils
{
    public class Loader
    {

        public static void Main(string[] args)
        {
            Graph graph = new Graph();
            List<Address> addresses = new List<Address>();
            LoadXML(ref graph, ref addresses, @"D:\Jann\Downloads\koeln_slim.osm");
        }

        public static void LoadXML(ref Graph graph, ref List<Address> addresses, string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            XmlReader reader = XmlReader.Create(path, settings);


            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.EndElement && reader.Name == "node")
                {
                    UInt64 nodeId = Convert.ToUInt64(reader.GetAttribute("id"));
                    float lat = Convert.ToSingle(reader.GetAttribute("lat").Replace('.', ','));
                    float lon = Convert.ToSingle(reader.GetAttribute("lon").Replace('.', ','));
                    Graph.Node newNode = new Graph.Node(nodeId, lat, lon);
                    graph.nodes.Add(nodeId, newNode);
                }
                else if (reader.NodeType != XmlNodeType.EndElement && reader.Name == "address")
                {
                    string countrycode = reader.GetAttribute("countrycode");
                    string cityname = reader.GetAttribute("cityname");
                    UInt16 postcode = Convert.ToUInt16(reader.GetAttribute("postcode"));
                    string streetname = reader.GetAttribute("streetname");
                    string housenumber = reader.GetAttribute("housenumber");
                    float lat = Convert.ToSingle(reader.GetAttribute("lat"));
                    float lon = Convert.ToSingle(reader.GetAttribute("lon"));
                    addresses.Add(new Address(countrycode, postcode, cityname, streetname, housenumber, lat, lon));
                }
            }

            reader.Close();
            reader = XmlReader.Create(path, settings);

            Graph.Node currentNode = null;
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.EndElement && reader.Name == "node")
                {
                    UInt64 nodeId = Convert.ToUInt64(reader.GetAttribute("id"));
                    currentNode = (Graph.Node)graph.nodes[nodeId];
                }else if(reader.NodeType != XmlNodeType.EndElement && reader.Name == "connection")
                {
                    double distance = Convert.ToDouble(reader.GetAttribute("distance"));
                    float timeNeeded = Convert.ToSingle(reader.GetAttribute("time"));
                    string type = reader.GetAttribute("type");

                    UInt64 nodeId = Convert.ToUInt64(reader.GetAttribute("id"));
                    Graph.Node neighbor = (Graph.Node)graph.nodes[nodeId];

                    Graph.Connection newConnection = new Graph.Connection(distance, timeNeeded, neighbor, type);
                    currentNode.AddConnection(newConnection);
                }
            }
            reader.Close();
        }
    }
}
