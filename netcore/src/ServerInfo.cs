using System.Collections.Generic;
using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("serverInfo")]
    public class ServerInfo
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("state")]
        public string State { get; set; }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }

        [XmlAttribute("connectedFor")]
        public int ConnectedFor { get; set; }

        [XmlAttribute("maxDevices")]
        public int MaxDevices { get; set; }

        [XmlAttribute("connectedId")]
        public int ConnectedId { get; set; }

        [XmlAttribute("interface")]
        public string Interface { get; set; }

        [XmlAttribute("serialNumber")]
        public string SerialNumber { get; set; }

        [XmlAttribute("easyFind")]
        public bool EasyFind { get; set; }

        [XmlAttribute("devices")]
        public List<DeviceInfo> Devices { get; set; }
    }
}
