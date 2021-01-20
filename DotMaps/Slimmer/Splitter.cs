using DotMaps.Datastructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DotMaps
{

    public partial class Converter
    {
        public struct Splitter
        {

            public static void SplitWays(Converter parent, string path, string newPath)
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


                parent.OnStatusChange?.Invoke(parent, new StatusChangedEventArgs
                {
                    status = "Counting occurances of nodes..."
                });
                int line = 0;
                Dictionary<string, string> tags = new Dictionary<string, string>();
                ulong id = 0;
                byte part = 0;
                reader.MoveToContent();
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
                                if (tags.ContainsKey("highway"))
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
                                    ulong nodeId = Convert.ToUInt64(reader.GetAttribute("id"));
                                    string lat = reader.GetAttribute("lat");
                                    string lon = reader.GetAttribute("lon");
                                    writer.WriteStartElement("node");
                                    writer.WriteAttributeString("id", nodeId.ToString());
                                    writer.WriteAttributeString("lat", lat);
                                    writer.WriteAttributeString("lon", lon);
                                    writer.WriteEndElement();
                                    break;
                                case "way":
                                    nodeType = WAY;
                                    currentNodes.Clear();
                                    tags.Clear();
                                    nodeId = Convert.ToUInt32(reader.GetAttribute("id"));
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
                                    ulong nodeId = Convert.ToUInt64(reader.GetAttribute("ref"));
                                    currentNodes.Add(nodeId);
                                    break;
                                case "tag":
                                    string key = reader.GetAttribute("k");
                                    if (copykeys.Contains(key))
                                        tags.Add(key, reader.GetAttribute("v").ToString());
                                    break;
                            }
                        }
                    }
                }
                reader.Close();

                parent.OnStatusChange?.Invoke(parent, new StatusChangedEventArgs
                {
                    status = "Splitting ways..."
                });
                line = 0;
                reader = XmlReader.Create(path, readerSettings);
                reader.MoveToContent();
                while (reader.Read())
                {
                    parent.OnProgress?.Invoke(parent, new ProgressEventArgs
                    {
                        progress = Math.DivRem(totalLines * 100, line++, out int rest)
                    });
                    if (reader.Depth == 1)
                    {
                        if (state == READINGNODES && nodeType == WAY)
                        {
                            state = NODEREAD;
                            if (tags.ContainsKey("highway"))
                            {
                                part = 0;

                                writer.WriteStartElement("way");
                                writer.WriteAttributeString("id", id.ToString());
                                writer.WriteAttributeString("part", part.ToString());
                                foreach (KeyValuePair<string, string> tag in tags)
                                {
                                    writer.WriteStartElement("tag");
                                    writer.WriteAttributeString("k", tag.Key);
                                    writer.WriteAttributeString("v", tag.Value);
                                    writer.WriteEndElement();
                                }

                                for (int i = 0; i < currentNodes.Count; i++)
                                {
                                    if (i > 0 && i < currentNodes.Count - 1 && nodeOccurances[currentNodes[i]] > 1)
                                    {
                                        writer.WriteStartElement("nd");
                                        writer.WriteAttributeString("ref", currentNodes[i].ToString());
                                        writer.WriteEndElement();
                                        writer.WriteEndElement();

                                        part++;

                                        writer.WriteStartElement("way");
                                        writer.WriteAttributeString("id", id.ToString());
                                        writer.WriteAttributeString("part", part.ToString());
                                        foreach (KeyValuePair<string, string> tag in tags)
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
                                tags.Clear();
                                id = Convert.ToUInt32(reader.GetAttribute("id"));
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
                                ulong nodeId = Convert.ToUInt64(reader.GetAttribute("ref"));
                                currentNodes.Add(nodeId);
                                break;
                            case "tag":
                                string key = reader.GetAttribute("k");
                                if (copykeys.Contains(key))
                                    tags.Add(key, reader.GetAttribute("v").ToString());
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
}
