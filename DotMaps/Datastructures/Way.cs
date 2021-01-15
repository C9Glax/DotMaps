using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public struct Way
    {
        public ulong id;
        public byte part;
        public List<Graph.GraphNode> nodes { get; }
        public Dictionary<string, string> tags { get; }

        public Way(ulong id)
        {
            this.id = id;
            this.part = 0;
            this.nodes = new List<Graph.GraphNode>();
            this.tags = new Dictionary<string, string>();
        }

        public Way(ulong id, byte part)
        {
            this.id = id;
            this.part = part;
            this.nodes = new List<Graph.GraphNode>();
            this.tags = new Dictionary<string, string>();
        }
    }
}
