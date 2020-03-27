using System;
using System.Collections.Generic;
using System.Text;

namespace DotMaps.Datastructures
{
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
