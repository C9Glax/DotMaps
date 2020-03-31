using System;

namespace DotMaps.Datastructures
{
    public class _2DNode
    {
        public float coordinateX, coordinateY;
        public UInt64 id;

        public _2DNode(UInt64 id, float coordinateX, float coordinateY)
        {
            this.coordinateX = coordinateX;
            this.coordinateY = coordinateY;
            this.id = id;
        }
    }
}
