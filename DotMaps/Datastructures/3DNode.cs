namespace DotMaps.Datastructures
{
    public struct _3DNode
    {
        public float lat { get; }
        public float lon { get; }

        public _3DNode(float lat, float lon)
        {
            this.lat = lat;
            this.lon = lon;
        }

        public override string ToString()
        {
            return string.Format("3DNode < {0} {1} >", lat, lon);
        }
    }
}
