using System.Collections;
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

        public void AddNode(GraphNode node)
        {
            this.nodes.Add(node.id, node);
        }

        public Hashtable GetNodes()
        {
            /*Node[] ret = new Node[this.nodes.Count];
            this.nodes.Values.CopyTo(ret, 0);*///REWRITE
            return this.nodes;
        }

        public GraphNode GetNode(ulong id)
        {
            return (GraphNode)this.nodes[id];
        }

        public bool ContainsNode(ulong id)
        {
            return this.nodes.ContainsKey(id);
        }

        public int GetNumberOfNodes()
        {
            return this.nodes.Count;
        }

        public struct GraphNode
        {
            public ulong id { get; }
            public float lat { get; }
            public float lon { get; }
            private List<Connection> connections;

            public GraphNode(ulong id, float lat, float lon)
            {
                this.id = id;
                this.lat = lat;
                this.lon = lon;
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
            public GraphNode neighbor { get; }
            public string type { get; }
            public string name { get; }

            public Connection(double distance, float timeNeeded, GraphNode neighbor, string type, string name)
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
