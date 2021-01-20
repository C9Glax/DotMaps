using DotMaps.Datatypes;
using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class Graph
    {
        public Dictionary<idinteger, GraphNode> nodes { get; }
        public float minLat { get; set; }
        public float maxLat { get; set; }
        public float minLon { get; set; }
        public float maxLon { get; set; }
        public Graph()
        {
            this.nodes = new Dictionary<idinteger, GraphNode>();
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

        public GraphNode GetNode(idinteger id)
        {
            if (!this.ContainsNodeWithId(id))
                throw new System.ArgumentOutOfRangeException("Node not in Graph.");
            return this.nodes[id];
        }

        public bool ContainsNodeWithId(idinteger id)
        {
            return this.nodes.ContainsKey(id);
        }

        public void RemoveNode(idinteger id)
        {
            if (!this.ContainsNodeWithId(id))
                throw new System.ArgumentOutOfRangeException("Node not in Graph.");
            this.nodes.Remove(id);
        }

        public class GraphNode
        {
            public idinteger id { get; }
            public _3DNode coordinates { get; }
            public List<Connection> connections { get; }
            public GraphNode previous { get; set; }
            public double weight { get; set; }

            public GraphNode(idinteger id, float lat, float lon)
            {
                this.id = id;
                this.coordinates = new _3DNode(lat, lon);
                this.connections = new List<Connection>();
                this.previous = null;
                this.weight = double.MaxValue;
            }

            public GraphNode(idinteger id, _3DNode position)
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
