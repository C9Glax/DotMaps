using System;
using System.Collections.Generic;
using System.Collections;

namespace DotMaps.Datastructures
{
    public class Way
    {
        public UInt64 id { get; }
        public List<UInt64> nodes { get; }
        public Hashtable tags { get; }

        public Way(UInt64 id)
        {
            this.id = id;
            this.nodes = new List<ulong>();
            this.tags = new Hashtable();
        }
    }
}
