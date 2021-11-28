using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("useInfo")]
    public class UseInfo
    {
        [XmlAttribute("userId")]
        public string UserId { get; set; }

        [XmlAttribute("username")]
        public string Username { get; set; }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("hostname")]
        public string Hostname { get; set; }
    }
}
