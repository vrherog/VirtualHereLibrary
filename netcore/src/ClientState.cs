using System.Collections.Generic;
using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("state")]
    public class ClientState
    {
        [XmlElement("server")]
        public List<Server> Servers { get; set; }
    }
}
