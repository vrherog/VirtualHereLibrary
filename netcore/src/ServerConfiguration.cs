namespace vrhero.VirtualHere
{
    public class ServerConfiguration: TransportSection
    {
        /// <summary>
        /// This is the TCP port the VirtualHere server runs on, the default if not specified is 7575.
        /// </summary>
        public ushort? TCPPort { get; set; }

        /// <summary>
        /// The name of the server that appears in the client e.g "Virtual USB Hub". 
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The license code for the server. 
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// This specifies the hostname of the server when communicating with clients. 
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// This specifies the script to run to authorize particular user/device combinations. 
        /// </summary>
        public string ClientAuthorization { get; set; }

        /// <summary>
        ///  This specifies the devices to ignore if they are plugged into the server. This setting is of the format xxxx[/yyyy],xxxx/yyyy,... 
        /// </summary>
        public string IgnoredDevices { get; set; }

        public TransportSection Transport { get; set; }
    }
}
