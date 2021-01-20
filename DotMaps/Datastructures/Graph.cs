using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class Graph
    {
        private Dictionary<ulong, GraphNode> nodes { get; }
        public float minLat { get; set; }
        public float maxLat { get; set; }
        public float minLon { get; set; }
        public float maxLon { get; set; }
        public Graph()
        {
            this.nodes = new Dictionary<ulong, GraphNode>();
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
            if (!this.ContainsNode(id))
                throw new System.ArgumentOutOfRangeException("Node not in Graph.");
            return this.nodes[id];
        }

        public bool ContainsNode(ulong id)
        {
            return this.nodes.ContainsKey(id);
        }

        public void RemoveNode(ulong id)
        {
            if (!this.ContainsNode(id))
                throw new System.ArgumentOutOfRangeException("Node not in Graph.");
            this.nodes.Remove(id);
        }

        public class GraphNode
        {
            public ulong id { get; }
            public _3DNode coordinates { get; }
            
            public List<Connection> connections { get; }

            public GraphNode previous;

            public double weight;

            public GraphNode(ulong id, float lat, float lon)
            {
                this.id = id;
                this.coordinates = new _3DNode(lat, lon);
                this.connections = new List<Connection>();
                this.previous = null;
                this.weight = double.MaxValue;
            }

            public GraphNode(ulong id, _3DNode position)
            {
                this.id = id;
                this.coordinates = position;
                this.connections = new List<Connection>();
                this.previous = null;
                this.weight = double.MaxValue;
            }
        }

        public class Connection
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
