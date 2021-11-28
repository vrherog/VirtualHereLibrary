using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("connection")]
    public class Connection
    {
        [XmlAttribute("connectionId")]
        public int Id { get; set; }

        [XmlAttribute("secure")]
        public bool Secure { get; set; }

        [XmlAttribute("serverMajor")]
        public int ServerMajor { get; set; }

        [XmlAttribute("serverMinor")]
        public int ServerMinor { get; set; }

        [XmlAttribute("serverRevision")]
        public int ServerRevision { get; set; }

        [XmlAttribute("remoteAdmin")]
        public bool RemoteAdmin { get; set; }

        [XmlAttribute("serverName")]
        public string ServerName { get; set; }

        [XmlAttribute("interfaceName")]
        public string InterfaceName { get; set; }

        [XmlAttribute("hostname")]
        public string Hostname { get; set; }

        [XmlAttribute("serverSerial")]
        public string ServerSerial { get; set; }

        [XmlAttribute("license_max_devices")]
        public int LicenseMaxDevices { get; set; }

        [XmlAttribute("state")]
        public int State { get; set; }

        [XmlAttribute("connectedTime")]
        public string ConnectedTime { get; set; }

        [XmlAttribute("host")]
        public string Host { get; set; }

        [XmlAttribute("port")]
        public ushort Port { get; set; }

        [XmlAttribute("error")]
        public bool HasError { get; set; }

        [XmlAttribute("uuid")]
        public string UUID { get; set; }

        [XmlAttribute("transportId")]
        public string TransportId { get; set; }

        [XmlAttribute("easyFindEnabled")]
        public bool EasyFindEnabled { get; set; }

        [XmlAttribute("easyFindAvailable")]
        public bool EasyFindAvailable { get; set; }

        [XmlAttribute("easyFindId")]
        public string EasyFindId { get; set; }

        [XmlAttribute("easyFindPin")]
        public string EasyFindPin { get; set; }

        [XmlAttribute("easyFindAuthorized")]
        public int EasyFindAuthorized { get; set; }

        [XmlAttribute("ip")]
        public string IP { get; set; }
    }
}
