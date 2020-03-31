using System;
using System.Collections;
using System.Collections.Generic;

namespace DotMaps.Datastructures
{
    public class Graph
    {
        public Hashtable nodes;
        public float minLat, maxLat, minLon, maxLon;
        public Graph()
        {
            this.nodes = new Hashtable();
        }
    }
}
