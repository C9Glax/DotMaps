using System;
using System.Collections.Generic;
using System.Text;

namespace DotMaps.Datastructures
{
    public class Connection
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
