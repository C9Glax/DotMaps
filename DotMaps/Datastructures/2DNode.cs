namespace DotMaps.Datastructures
{
    public struct _2DNode
    {
        public float coordinateX { get; }
        public float coordinateY { get; }
        public ulong id;

        public _2DNode(ulong id, float coordinateX, float coordinateY)
        {
            this.coordinateX = coordinateX;
            this.coordinateY = coordinateY;
            this.id = id;
        }
    }
}
