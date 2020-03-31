namespace DotMaps.Datastructures
{
    public struct _3DNode
    {
        public ulong id { get; }
        public float lat { get; }
        public float lon { get; }

        public _3DNode(ulong id, float lat, float lon)
        {
            this.id = id;
            this.lat = lat;
            this.lon = lon;
        }
    }
}
