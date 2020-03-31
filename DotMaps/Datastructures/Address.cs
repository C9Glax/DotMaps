namespace DotMaps.Datastructures
{
    public struct Address
    {
        public ulong assosciatedNode { get; set; }
        public string cityname { get; }
        public string postcode { get; }
        public string steetname { get; }
        public string housenumber { get; }
        public string country { get; }
        public float lat { get; }
        public float lon { get; }
        public Address(string country, string postcode, string cityname, string streetname, string housenumber, float lat, float lon)
        {
            this.steetname = streetname;
            this.housenumber = housenumber;
            this.postcode = postcode;
            this.cityname = cityname;
            this.country = country;
            this.lat = lat;
            this.lon = lon;
            this.assosciatedNode = 0;
        }

        public Address(string country, string postcode, string cityname, string streetname, string housenumber, float lat, float lon, ulong node)
        {
            this.steetname = streetname;
            this.housenumber = housenumber;
            this.postcode = postcode;
            this.cityname = cityname;
            this.country = country;
            this.lat = lat;
            this.lon = lon;
            this.assosciatedNode = node;
        }
    }
}
