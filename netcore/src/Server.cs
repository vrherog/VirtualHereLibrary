using System.Collections.Generic;
using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("server")]
    public class Server
    {
        [XmlElement("connection")]
        public Connection Connection { get; set; }

        [XmlElement("device")]
        public List<Device> Devices { get; set; }
    }
}
