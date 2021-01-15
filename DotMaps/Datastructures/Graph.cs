using System.Collections;
using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class Graph
    {
        private Hashtable nodes { get; }
        public float minLat, maxLat, minLon, maxLon;
        public Graph()
        {
            this.nodes = new Hashtable();
        }

        public void AddNode(GraphNode node)
        {
            this.nodes.Add(node.id, node);
        }

        public GraphNode[] GetNodes()
        {
            GraphNode[] ret = new GraphNode[this.nodes.Count];
            this.nodes.Values.CopyTo(ret, 0);
            return ret;
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

        public void RemoveNode(ulong id)
        {
            this.nodes.Remove(id);
        }

        public class GraphNode
        {
            public ulong id { get; }

            public _3DNode position { get; }
            
            private List<Connection> connections { get; }

            public GraphNode previous;

            public double weight;

            public GraphNode(ulong id, float lat, float lon)
            {
                this.id = id;
                this.position = new _3DNode(lat, lon);
                this.connections = new List<Connection>();
                this.previous = null;
                this.weight = double.MaxValue;
            }

            public GraphNode(uint id, _3DNode position)
            {
                this.id = id;
                this.position = position;
                this.connections = new List<Connection>();
                this.previous = null;
                this.weight = double.MaxValue;
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
            public string name { get; }
            public _3DNode[] coordinates { get; }

            public string roadType { get; }

            public Connection(double distance, float timeNeeded, GraphNode neighbor, string name, _3DNode[] coordinates, string roadType)
            {
                this.distance = distance;
                this.timeNeeded = timeNeeded;
                this.neighbor = neighbor;
                this.name = name;
                this.coordinates = coordinates;
                this.roadType = roadType;
            }
        }
    }
}
