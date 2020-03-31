using System;
using System.Collections.Generic;
using System.Collections;

namespace DotMaps.Datastructures
{
    public struct Way
    {
        public UInt64 id;
        public List<_3DNode> nodes { get; }
        public Hashtable tags { get; }

        public Way(UInt64 id)
        {
            this.id = id;
            this.nodes = new List<_3DNode>();
            this.tags = new Hashtable();
        }
    }
}
