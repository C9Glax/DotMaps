using DotMaps.Datastructures;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;

namespace DotMaps
{
    public partial class Converter
    {
        public struct Slimmer
        {
            public static void SlimOSMFormat(Converter parent, string path, string newPath)
            {
                int totalLines = 0;
                if (parent.OnProgress != null)
                {
                    parent.OnStatusChange?.Invoke(parent, new StatusChangedEventArgs
                    {
                        status = string.Format("Counting Lines")
                    });
                    using (TextReader linereader = new StreamReader(path))
                        while (linereader.ReadLine() != null)
                            totalLines++;
                }

                const byte UNKNOWN = 0, NODE = 1, WAY = 2, READINGNODES = 1, NODEREAD = 0;
                byte nodeType = UNKNOWN, state = NODEREAD;
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

                HashSet<ulong> neededNodesIds = new HashSet<ulong>();
                List<ulong> currentNodes = new List<ulong>();
                Way currentWay = new Way(0);
                int line = 0;
                parent.OnStatusChange?.Invoke(parent, new StatusChangedEventArgs
                {
                    status = "Reading and Cleaning Ways"
                });
                while (reader.Read())
                {
                    parent.OnProgress?.Invoke(parent, new ProgressEventArgs
                    {
                        progress = Math.DivRem(totalLines * 100, line++, out int rest)
                    });
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.Depth == 1)
                        {
                            if (state == READINGNODES && nodeType == WAY)
                            {
                                state = NODEREAD;
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
                                        currentWay.tags.Add(key, reader.GetAttribute("v"));
                                    break;
                            }
                        }
                    }
                }
                reader.Close();

                int countCopiedNodes = 0;
                reader = XmlReader.Create(path, readerSettings);
                parent.OnStatusChange?.Invoke(parent, new StatusChangedEventArgs
                {
                    status = "Copying Necessary Nodes"
                });
                while (reader.Read())
                {
                    parent.OnProgress?.Invoke(parent, new ProgressEventArgs
                    {
                        progress = Math.DivRem(countCopiedNodes * 100, neededNodesIds.Count, out int rest)
                    });
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
                writer.Close();
                parent.OnStatusChange?.Invoke(parent, new StatusChangedEventArgs
                {
                    status = "Done."
                });
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
    
}
