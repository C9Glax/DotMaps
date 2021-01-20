using DotMaps.Datatypes;
namespace DotMaps.Datastructures
{
    public struct Address
    {
        public idinteger assosciatedNode { get; set; }
        public string cityname { get; }
        public string postcode { get; }
        public string steetname { get; }
        public string housenumber { get; }
        public string country { get; }
        public _3DNode position { get; }

        public Address(string country, string postcode, string cityname, string streetname, string housenumber, _3DNode position)
        {
            this.steetname = streetname;
            this.housenumber = housenumber;
            this.postcode = postcode;
            this.cityname = cityname;
            this.country = country;
            this.position = position;
            this.assosciatedNode = new idinteger(0);
        }

        public Address(string country, string postcode, string cityname, string streetname, string housenumber, _3DNode position, idinteger nodeId)
        {
            this.steetname = streetname;
            this.housenumber = housenumber;
            this.postcode = postcode;
            this.cityname = cityname;
            this.country = country;
            this.position = position;
            this.assosciatedNode = nodeId;
        }
    }
}
