using System;
using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class _3DNode
    {
        public UInt64 id { get; }
        public float lat { get; }
        public float lon { get; }

        private List<Connection> connections;
        public _3DNode previousNode { get; set; }
        public double timeRequired { get; set; }

        public _3DNode(UInt64 id, float lat, float lon)
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
}
