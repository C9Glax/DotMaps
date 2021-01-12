namespace DotMaps.Datastructures
{
    public struct _2DNode
    {
        public float X { get; }
        public float Y { get; }
        public ulong id;

        public _2DNode(ulong id, float coordinateX, float coordinateY)
        {
            this.X = coordinateX;
            this.Y = coordinateY;
            this.id = id;
        }

        public _2DNode(float coordinateX, float coordinateY)
        {
            this.X = coordinateX;
            this.Y = coordinateY;
            this.id = 0;
        }
    }
}
