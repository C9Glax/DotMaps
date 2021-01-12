using DotMaps.Datastructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DotMaps.Converter
{
    public class Splitter
    {
        public void SplitWays(Converter parent, string path, string newPath)
        {
            const byte UNKNOWN = 0, NODE = 1, WAY = 2, READINGNODES = 1, NODEREAD = 0;
            byte nodeType = UNKNOWN, state = NODEREAD;

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

            Dictionary<ulong, uint> nodeOccurances = new Dictionary<ulong, uint>();
            List<ulong> currentNodes = new List<ulong>();
            List<string> copykeys = new List<string>();
            foreach (string key in File.ReadAllLines("copykeys.txt"))
                copykeys.Add(key);
            Way currentWay = new Way(0);
            uint totalLines = 0;
            reader.MoveToContent();
            while (reader.Read())
            {
                parent.status = "Counting occurances of nodes: " + ++totalLines + " Nodetype: " + reader.Name + "   ";
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.Depth == 1)
                    {
                        if (state == READINGNODES && nodeType == WAY)
                        {
                            state = NODEREAD;
                            if (currentWay.tags.ContainsKey("highway"))
                                foreach (ulong nodeID in currentNodes)
                                    if (!nodeOccurances.ContainsKey(nodeID))
                                        nodeOccurances.Add(nodeID, 1);
                                    else
                                        nodeOccurances[nodeID]++;
                        }
                        switch (reader.Name)
                        {
                            case "node":
                                nodeType = NODE;
                                ulong id = Convert.ToUInt64(reader.GetAttribute("id"));
                                string lat = reader.GetAttribute("lat");
                                string lon = reader.GetAttribute("lon");
                                writer.WriteStartElement("node");
                                writer.WriteAttributeString("id", id.ToString());
                                writer.WriteAttributeString("lat", lat);
                                writer.WriteAttributeString("lon", lon);
                                writer.WriteEndElement();
                                break;
                            case "way":
                                nodeType = WAY;
                                currentNodes.Clear();
                                currentWay.tags.Clear();
                                currentWay.id = Convert.ToUInt64(reader.GetAttribute("id"));
                                break;
                            default:
                                nodeType = UNKNOWN;
                                break;
                        }
                    }
                    else if (reader.Depth == 2 && nodeType == WAY)
                    {
                        state = READINGNODES;
                        switch (reader.Name)
                        {
                            case "nd":
                                ulong id = Convert.ToUInt64(reader.GetAttribute("ref"));
                                currentNodes.Add(id);
                                break;
                            case "tag":
                                string key = reader.GetAttribute("k");
                                if (copykeys.Contains(key))
                                    currentWay.tags.Add(key, reader.GetAttribute("v").ToString());
                                break;
                        }
                    }
                }
            }
            reader.Close();
            Console.WriteLine("Counted occurances");

            Console.WriteLine("Splitting ways");
            uint line = 0;
            reader = XmlReader.Create(path, readerSettings);
            reader.MoveToContent();
            while (reader.Read())
            {
                parent.status = "Splitting ways: " + ++line + "/" + totalLines ;
                if (reader.Depth == 1)
                {
                    if (state == READINGNODES && nodeType == WAY)
                    {
                        state = NODEREAD;
                        if (currentWay.tags.ContainsKey("highway"))
                        {
                            currentWay.part = 0;

                            writer.WriteStartElement("way");
                            writer.WriteAttributeString("id", currentWay.id.ToString());
                            writer.WriteAttributeString("part", currentWay.part.ToString());
                            foreach (KeyValuePair<string, string> tag in currentWay.tags)
                            {
                                writer.WriteStartElement("tag");
                                writer.WriteAttributeString("k", tag.Key);
                                writer.WriteAttributeString("v", tag.Value);
                                writer.WriteEndElement();
                            }

                            for(int i = 0; i < currentNodes.Count; i++)
                            {
                                if (i > 0 && i < currentNodes.Count - 1 && nodeOccurances[currentNodes[i]] > 1)
                                {
                                    writer.WriteStartElement("nd");
                                    writer.WriteAttributeString("ref", currentNodes[i].ToString());
                                    writer.WriteEndElement();
                                    writer.WriteEndElement();

                                    currentWay.part++;

                                    writer.WriteStartElement("way");
                                    writer.WriteAttributeString("id", currentWay.id.ToString());
                                    writer.WriteAttributeString("part", currentWay.part.ToString());
                                    foreach (KeyValuePair<string, string> tag in currentWay.tags)
                                    {
                                        writer.WriteStartElement("tag");
                                        writer.WriteAttributeString("k", tag.Key);
                                        writer.WriteAttributeString("v", tag.Value);
                                        writer.WriteEndElement();
                                    }

                                    writer.WriteStartElement("nd");
                                    writer.WriteAttributeString("ref", currentNodes[i].ToString());
                                    writer.WriteEndElement();
                                }
                                else
                                {
                                    writer.WriteStartElement("nd");
                                    writer.WriteAttributeString("ref", currentNodes[i].ToString());
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement();
                        }
                    }
                    switch (reader.Name)
                    {
                        case "node":
                            nodeType = NODE;
                            break;
                        case "way":
                            nodeType = WAY;
                            currentNodes.Clear();
                            currentWay.tags.Clear();
                            currentWay.id = Convert.ToUInt64(reader.GetAttribute("id"));
                            break;
                        default:
                            nodeType = UNKNOWN;
                            break;
                    }
                }
                else if (reader.Depth == 2 && nodeType == WAY)
                {
                    state = READINGNODES;
                    switch (reader.Name)
                    {
                        case "nd":
                            ulong id = Convert.ToUInt64(reader.GetAttribute("ref"));
                            currentNodes.Add(id);
                            break;
                        case "tag":
                            string key = reader.GetAttribute("k");
                            if (copykeys.Contains(key))
                                currentWay.tags.Add(key, reader.GetAttribute("v").ToString());
                            break;
                    }
                }
            }
            reader.Close();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
    }
}
