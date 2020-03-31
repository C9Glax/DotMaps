﻿using System.Collections;
using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class Graph
    {
        private Hashtable nodes;
        public float minLat, maxLat, minLon, maxLon;
        public Graph()
        {
            this.nodes = new Hashtable();
        }

        public void AddNode(Node node)
        {
            this.nodes.Add(node.id, node);
        }

        public Node[] GetNodes()
        {
            Node[] ret = new Node[this.nodes.Count];
            this.nodes.Values.CopyTo(ret, 0);
            return ret;
        }

        public Node GetNode(ulong id)
        {
            return (Node)this.nodes[id];
        }

        public bool ContainsNode(ulong id)
        {
            return this.nodes.ContainsKey(id);
        }

        public int GetNumberOfNodes()
        {
            return this.nodes.Count;
        }

        public struct Node
        {
            public ulong id { get; }
            private List<Connection> connections;

            public Node(ulong id)
            {
                this.id = id;
                this.connections = new List<Connection>();
            }

            public void AddConnection(Connection connection)
            {
                this.connections.Add(connection);
            }

            public Connection[] GetConnections()
            {
                return this.connections.ToArray();
            }
        }

        public struct Connection
        {
            public double distance { get; }
            public float timeNeeded { get; }
            public _3DNode neighbor { get; }
            public string type { get; }
            public string name { get; }

            public Connection(double distance, float timeNeeded, _3DNode neighbor, string type, string name)
            {
                this.distance = distance;
                this.timeNeeded = timeNeeded;
                this.neighbor = neighbor;
                this.type = type;
                this.name = name;
            }
        }
    }
}
