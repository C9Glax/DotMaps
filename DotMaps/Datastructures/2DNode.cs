namespace DotMaps.Datastructures
{
    public struct _2DNode
    {
        public float X { get; }
        public float Y { get; }

        public _2DNode(float coordinateX, float coordinateY)
        {
            this.X = coordinateX;
            this.Y = coordinateY;
        }

        public override string ToString()
        {
            return string.Format("2DNode < {0} {1} >", this.X, this.Y);
        }
    }
}
