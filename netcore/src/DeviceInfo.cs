using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    [XmlType("deviceInfo")]

    public class DeviceInfo
    {
        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("vendorId")]
        public int VendorId { get; set; }

        [XmlAttribute("vendor")]
        public string Vendor { get; set; }

        [XmlAttribute("productId")]
        public int ProductId { get; set; }

        [XmlAttribute("product")]
        public string Product { get; set; }

        [XmlAttribute("serialNumber")]
        public string SerialNumber { get; set; }

        [XmlAttribute("inUseBy")]
        public UseInfo InUseBy { get; set; }

        [XmlIgnore]
        public bool IsConnected { get; set; }
    }
}
