﻿using DotMaps.Datastructures;
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
            Thread statusPrinterThread = new Thread(StatusThread);
            statusPrinterThread.Start(slimmer);
            slimmer.SlimOSMFormat(path, newPath);
            statusPrinterThread.Abort();
        }

        string status = "";

        public static void StatusThread(object slimmerObject)
        {
            Slimmer slimmer = (Slimmer)slimmerObject;
            int line;
            while (Thread.CurrentThread.ThreadState != ThreadState.AbortRequested)
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
            foreach (string key in File.ReadAllLines("copykeys.txt"))
                copykeys.Add(key);

            XmlReaderSettings readerSettings = new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            };
            XmlReader reader = XmlReader.Create(path, readerSettings);

            XmlWriterSettings writerSettings = new XmlWriterSettings()
            {
                Encoding = System.Text.Encoding.UTF8,
                Indent = true
            };
            XmlWriter writer = XmlWriter.Create(newPath, writerSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("osm");
            writer.WriteAttributeString("version", "0.6");
            writer.WriteAttributeString("generator", "OSMSlimmer");
            writer.WriteAttributeString("copyright", "OpenStreetMap and contributors");
            writer.WriteAttributeString("attribtion", "http://www.openstreetmap.org/copyright");
            writer.WriteAttributeString("license", "http://opendatacommons.org/licenses/odbl/1-0/");

            Console.WriteLine("Reading and Cleaning ways");

            HashSet<ulong> neededNodesIds = new HashSet<ulong>();
            List<ulong> currentNodes = new List<ulong>();
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
                            writer.WriteStartElement("way");
                            writer.WriteAttributeString("id", currentWay.id.ToString());
                            if (currentWay.tags.ContainsKey("highway"))
                            {
                                foreach (ulong nodeID in currentNodes)
                                {
                                    writer.WriteStartElement("nd");
                                    writer.WriteAttributeString("ref", nodeID.ToString());
                                    writer.WriteEndElement();
                                    if (!neededNodesIds.Contains(nodeID))
                                        neededNodesIds.Add(nodeID);
                                }
                            }
                            else if (currentWay.tags.ContainsKey("addr:housenumber") && !neededNodesIds.Contains(currentNodes[0]))
                            {
                                neededNodesIds.Add(currentNodes[0]);

                                writer.WriteStartElement("nd");
                                writer.WriteAttributeString("ref", currentNodes[0].ToString());
                                writer.WriteEndElement();
                            }
                            foreach (string key in currentWay.tags.Keys)
                            {
                                writer.WriteStartElement("tag");
                                writer.WriteAttributeString("k", key);
                                writer.WriteAttributeString("v", Cleaner(currentWay.tags[key].ToString()));
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
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
                                writer.WriteStartElement("bounds");
                                writer.WriteAttributeString("minlat", reader.GetAttribute("minlat"));
                                writer.WriteAttributeString("minlon", reader.GetAttribute("minlon"));
                                writer.WriteAttributeString("maxlat", reader.GetAttribute("maxlat"));
                                writer.WriteAttributeString("maxlon", reader.GetAttribute("maxlon"));
                                writer.WriteEndElement();
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
                                ulong id = Convert.ToUInt64(reader.GetAttribute("ref"));
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

            Console.WriteLine("Copying Necessary Nodes");

            uint countCopiedNodes = 0;
            reader = XmlReader.Create(path, readerSettings);
            while (reader.Read())
            {
                this.status = "Copying Necessary Nodes: " + countCopiedNodes + "/" + neededNodesIds.Count;
                if (reader.NodeType != XmlNodeType.EndElement && reader.Depth == 1 && reader.Name == "node")
                {
                    ulong id = Convert.ToUInt64(reader.GetAttribute("id"));
                    string lat = reader.GetAttribute("lat");
                    string lon = reader.GetAttribute("lon");
                    if (neededNodesIds.Contains(id))
                    {
                        writer.WriteStartElement("node");
                        writer.WriteAttributeString("id", id.ToString());
                        writer.WriteAttributeString("lat", lat);
                        writer.WriteAttributeString("lon", lon);
                        writer.WriteEndElement();
                        countCopiedNodes++;
                    }
                }
            }
            reader.Close();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            Console.WriteLine("Flushing");
            this.status = "Done. You can now close the program.";
        }

        private static string Cleaner(string input)
        {
            input = input.Replace("&", "und");
            input = input.Replace('"', ' ');
            input = input.Replace('<', ' ');
            input = input.Replace('>', ' ');
            
            return input;
            
        }
    }
}
