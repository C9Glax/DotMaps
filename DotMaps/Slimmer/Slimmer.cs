using DotMaps.Utils;
using DotMaps.Datastructures;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using System.Threading;

namespace DotMaps
{
    public class Slimmer
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Path to .osm");
            string path = Console.ReadLine();
            Console.WriteLine("Path to save _slim.osm");
            string newPath = Console.ReadLine();
            Slimmer slimmer = new Slimmer();
            Thread statusPrinterThread = new Thread(statusThread);
            statusPrinterThread.Start(slimmer);
            slimmer.SlimOSMFormat(path, newPath);
        }

        string status = "";

        public static void statusThread(object slimmerObject)
        {
            Slimmer slimmer = (Slimmer)slimmerObject;
            int line = Console.CursorTop;
            while (true)
            {
                line = Console.CursorTop;
                Console.WriteLine(slimmer.status);
                Console.CursorTop = line;
                Thread.Sleep(100);
            }
        }

        
        public void SlimOSMFormat(string path, string newPath)
        {
            const byte UNKNOWN = 0, NODE = 1, WAY = 2, READING = 1, DONE = 0;
            byte nodeType = UNKNOWN, state = DONE;
            List<string> copykeys = new List<string>();
            copykeys.AddRange(new string[] { "addr:city", "addr:housenumber", "addr:postcode", "addr:street", "addr:country", "highway", "oneway", "mayspeed", "name" });

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            XmlReader reader = XmlReader.Create(path, settings);

            FileStream outfile = new FileStream(newPath, FileMode.Create);
            StreamWriter writer = new StreamWriter(outfile, System.Text.Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            writer.WriteLine("<osm version=\"0.6\" generator=\"OSMSlimmer\" copyright=\"OpenStreetMap and contributors\" attribution=\"http://www.openstreetmap.org/copyright\" license=\"http://opendatacommons.org/licenses/odbl/1-0/\">");

            Console.WriteLine("Reading and Cleaning ways");

            HashSet<UInt64> neededNodesIds = new HashSet<UInt64>();
            List<UInt64> currentNodes = new List<ulong>();
            Way currentWay = new Way(0);
            uint line = 0;
            while (reader.Read())
            {
                this.status = "Reading and Cleaning Ways Line: " + ++line + " Nodetype: " + reader.Name + "   ";
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.Depth == 1)
                    {
                        if(state == READING && nodeType == WAY)
                        {
                            state = DONE;
                            writer.WriteLine("  <way id=\"" + currentWay.id + "\">");
                            if (currentWay.tags.ContainsKey("highway"))
                            {
                                foreach (ulong nodeID in currentNodes)
                                {
                                    writer.WriteLine("    <nd ref=\"" + nodeID + "\" />");
                                    if (!neededNodesIds.Contains(nodeID))
                                        neededNodesIds.Add(nodeID);
                                }
                            }
                            else if (currentWay.tags.ContainsKey("addr:housenumber") && !neededNodesIds.Contains(currentNodes[0]))
                            {
                                neededNodesIds.Add(currentNodes[0]);
                                writer.WriteLine("    <nd ref=\"" + currentNodes[0] + "\" />");
                            }
                            foreach (string key in currentWay.tags.Keys)
                                writer.WriteLine("    <tag k=\"" + key + "\" v=\"" + Cleaner(currentWay.tags[key].ToString()) + "\" />");
                            writer.WriteLine("  </way>");
                        }
                        switch (reader.Name)
                        {
                            case "node":
                                nodeType = NODE;
                                break;
                            case "way":
                                currentNodes.Clear();
                                currentWay.tags.Clear();
                                currentWay.id = Convert.ToUInt64(reader.GetAttribute("id"));
                                nodeType = WAY;
                                break;
                            case "bounds":
                                nodeType = UNKNOWN;
                                writer.WriteLine("  <bounds minlat=\"" + reader.GetAttribute("minlat") + "\" minlon=\"" + reader.GetAttribute("minlon") + "\" maxlat=\"" + reader.GetAttribute("maxlat") + "\" maxlon=\"" + reader.GetAttribute("maxlon") + "\" />");
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
                                UInt64 id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                currentNodes.Add(id);
                                break;
                            case "tag":
                                string key = reader.GetAttribute("k");
                                if (copykeys.Contains(key))
                                    currentWay.tags.Add(key, reader.GetAttribute("v"));
                                break;
                        }
                    }
                }
            }
            reader.Close();
            Console.WriteLine("Flushing");
            writer.Flush();

            Console.WriteLine("Copying Necessary Nodes");

            uint countCopiedNodes = 0;
            reader = XmlReader.Create(path, settings);
            while (reader.Read())
            {
                this.status = "Copying Necessary Nodes: " + countCopiedNodes + "/" + neededNodesIds.Count;
                if (reader.NodeType != XmlNodeType.EndElement && reader.Depth == 1 && reader.Name == "node")
                {
                    UInt64 id = Convert.ToUInt64(reader.GetAttribute("id"));
                    string lat = reader.GetAttribute("lat");
                    string lon = reader.GetAttribute("lon");
                    if (neededNodesIds.Contains(id))
                    {
                        writer.WriteLine("  <node id=\"" + id + "\" lat=\"" + lat + "\" lon=\"" + lon + "\" />");
                        countCopiedNodes++;
                    }
                }
            }
            reader.Close();

            writer.WriteLine("</osm>");
            Console.WriteLine("Flushing");
            writer.Flush();
            writer.Close();
            this.status = "Done. You can now close the program.";
        }

        private static string Cleaner(string input)
        {
            return input.Replace("&", "und").Replace('"', ' ');
            
        }
    }
}
