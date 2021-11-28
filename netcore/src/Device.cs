using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("device")]
    public class Device
    {
        [XmlAttribute("vendor")]
        public string Vendor { get; set; }

        [XmlAttribute("idVendor")]
        public int VendorId { get; set; }

        [XmlAttribute("product")]
        public string Product { get; set; }

        [XmlAttribute("idProduct")]
        public int ProductId { get; set; }

        [XmlAttribute("address")]
        public int Address { get; set; }

        [XmlAttribute("connectionId")]
        public int ConnectionId { get; set; }

        [XmlAttribute("state")]
        public int State { get; set; }

        [XmlAttribute("serverSerial")]
        public string ServerSerial { get; set; }

        [XmlAttribute("serverName")]
        public string ServerName { get; set; }

        [XmlAttribute("serverInterfaceName")]
        public string ServerInterfaceName { get; set; }

        [XmlAttribute("deviceSerial")]
        public string DeviceSerial { get; set; }

        [XmlAttribute("connectionUUID")]
        public string ConnectionUUID { get; set; }

        [XmlAttribute("boundConnectionUUID")]
        public string BoundConnectionUUID { get; set; }

        [XmlAttribute("boundConnectionIp")]
        public string BoundConnectionIp { get; set; }

        [XmlAttribute("boundConnectionIp6")]
        public string BoundConnectionIp6 { get; set; }

        [XmlAttribute("boundClientHostname")]
        public string BoundClientHostname { get; set; }

        [XmlAttribute("nickname")]
        public string Nickname { get; set; }

        [XmlAttribute("clientId")]
        public string ClientId { get; set; }

        [XmlAttribute("numConfigurations")]
        public int NumConfigurations { get; set; }

        [XmlAttribute("numInterfacesInFirstConfiguration")]
        public int NumInterfacesInFirstConfiguration { get; set; }

        [XmlAttribute("firstInterfaceClass")]
        public int FirstInterfaceClass { get; set; }

        [XmlAttribute("firstInterfaceSubClass")]
        public int FirstInterfaceSubClass { get; set; }

        [XmlAttribute("firstInterfaceProtocol")]
        public int FirstInterfaceProtocol { get; set; }

        [XmlAttribute("hideClientInfo")]
        public bool HideClientInfo { get; set; }

        [XmlAttribute("autoUse")]
        public string AutoUse { get; set; }

        public string GetDeviceId()
        {
            return Utils.HashMd5(string.IsNullOrWhiteSpace(DeviceSerial) ?
                $"{ServerSerial}:{Address}-{VendorId}:{ProductId}-{FirstInterfaceClass}:{FirstInterfaceSubClass}:{FirstInterfaceProtocol}:{NumInterfacesInFirstConfiguration}"
                : $"{VendorId}:{ProductId}-{DeviceSerial}");
        }
    }
}
