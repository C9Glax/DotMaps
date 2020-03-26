using System;
using System.Collections;
using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class Graph
    {
        public Hashtable nodes;
        public Graph()
        {
            this.nodes = new Hashtable();
        }

        public class Node
        {
            public UInt64 id { get; }
            public float lat { get; }
            public float lon { get; }
            private List<Connection> connections;
            public Node previousNode { get; set; }
            public double timeRequired { get; set; }

            public Node(UInt64 id, float lat, float lon)
            {
                this.id = id;
                this.lat = lat;
                this.lon = lon;
                this.timeRequired = double.MaxValue;
                this.connections = new List<Connection>();
            }

            public void AddConnection(Connection neighbor)
            {
                this.connections.Add(neighbor);
            }

            public Connection[] GetConnections()
            {
                return this.connections.ToArray();
            }
        }

        public class Connection
        {
            public double distance { get; }
            public float timeNeeded { get; }
            public Node neighbor { get; }
            public string type { get; }
            public Connection(double distance, float timeNeeded, Node neighbor, string type)
            {
                this.distance = distance;
                this.timeNeeded = timeNeeded;
                this.neighbor = neighbor;
                this.type = type;
            }
        }
    }
}
