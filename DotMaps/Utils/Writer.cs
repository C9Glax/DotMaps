using System.Collections.Generic;
using System.Collections;
using DotMaps.Datastructures;
using System.IO;

namespace DotMaps.Utils
{
    public class Writer
    {

        public static void WriteOSMXml(string path, string bounds, Hashtable nodes, List<Way> ways)
        {
            using(StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<osm version=\"0.6\" generator=\"OSMSlimmer\" copyright=\"OpenStreetMap and contributors\" attribution=\"http://www.openstreetmap.org/copyright\" license=\"http://opendatacommons.org/licenses/odbl/1-0/\">");
                writer.WriteLine(bounds);
                foreach(Node node in nodes.Values)
                    writer.WriteLine("  <node id=\"" + node.id + "\" lat=\"" + node.lat.ToString().Replace(',','.') + "\" lon=\"" + node.lon.ToString().Replace(',', '.') + "\" />");
                foreach(Way way in ways)
                {
                    writer.WriteLine("  <way id=\"" + way.id + "\">");
                    foreach(ulong node in way.nodes)
                        writer.WriteLine("    <nd ref=\"" + node + "\" />");
                    foreach (string key in way.tags.Keys)
                        writer.WriteLine("    <tag k=\"" + key + "\" v=\"" + (string)way.tags[key] + "\" />");
                    writer.WriteLine("  </way>");
                }
                writer.WriteLine("</osm>");
                writer.Close();
            }
        }

        public static void WriteCalculatedXml(string path, string bounds, Graph graph, List<Address> addresses)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<slim version=\"0.1\" generator=\"OSMSlimmer\">");
                writer.WriteLine(bounds);
                    //HERE
                writer.WriteLine("</slim>");
                writer.Close();
            }
        }
    }
}
