using System.Collections.Generic;
using System.Collections;

namespace DotMaps.Datastructures
{
    public struct Way
    {
        public ulong id;
        public List<Graph.GraphNode> nodes { get; }
        public Hashtable tags { get; }

        public Way(ulong id)
        {
            this.id = id;
            this.nodes = new List<Graph.GraphNode>();
            this.tags = new Hashtable();
        }
    }
}
