using System;

namespace DotMaps.Datastructures
{
    public class Address
    {
        public UInt64 assosciatedNode { get; set; }
        public string cityname { get; }
        public UInt16 postcode { get; }
        public string steetname { get; }
        public string housenumber { get; }
        public string country { get; }
        public float lat { get; }
        public float lon { get; }
        public Address(string country, UInt16 postcode, string cityname, string streetname, string housenumber, float lat, float lon)
        {
            this.steetname = streetname;
            this.housenumber = housenumber;
            this.postcode = postcode;
            this.cityname = cityname;
            this.country = country;
            this.lat = lat;
            this.lon = lon;
        }

        public Address(string country, UInt16 postcode, string cityname, string streetname, string housenumber, float lat, float lon, UInt64 node)
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
