namespace vrhero.VirtualHere
{
    static class Constants
    {
        public const int DefaultTimeout = 1000;

        public const string DefaultPipeServer = ".";
        public const string DefaultPipeName = "vhclient";

        public const string DefaultInputStreamFile = "/tmp/vhclient";
        public const string DefaultOutputStreamFile = "/tmp/vhclient_response";

        /// <summary>
        /// List devices
        /// </summary>
        public const string CommandList = "LIST\n";
        /// <summary>
        /// Get the detailed full client state as an XML Document
        /// </summary>
        public const string CommandGetClientState = "GET CLIENT STATE\n";
        /// <summary>
        /// Use a device:    "USE,<address>[,password]"
        /// </summary>
        public const string CommandUseDevice = "USE,{0}";
        /// <summary>
        /// Stop using a device:    "STOP USING,<address>"
        /// </summary>
        public const string CommandStopUsing = "STOP USING,{0}";
        /// <summary>
        /// Stop using all devices on all clients
        /// </summary>
        public const string CommandStopUsingAll = "STOP USING ALL\n";
        /// <summary>
        /// Stop using all devices just for this client
        /// </summary>
        public const string CommandStopUsingAllLocal = "STOP USING ALL LOCAL\n";
        /// <summary>
        /// Device Information:    "DEVICE INFO,<address>"
        /// </summary>
        public const string CommandDeviceInfo = "DEVICE INFO,{0}";
        /// <summary>
        /// Server Information:    "SERVER INFO,<server name>"
        /// </summary>
        public const string CommandServerInfo = "SERVER INFO,{0}";
        /// <summary>
        /// Set device nickname:    "DEVICE RENAME,<address>,<nickname>"
        /// </summary>
        public const string CommandDeviceRename = "DEVICE RENAME,{0},{1}";
        /// <summary>
        /// Rename server:    "SERVER RENAME,<hubaddress:port>,<new name>"
        /// </summary>
        public const string CommandServerRename = "SERVER RENAME,{0},{1}";
        /// <summary>
        /// Turn auto-use all devices on
        /// </summary>
        public const string CommandAutoUseAll = "AUTO USE ALL\n";
        /// <summary>
        /// Turn Auto-use all devices on this hub on/off:    "AUTO USE HUB,<server name>"
        /// </summary>
        public const string CommandAutoUseHub = "AUTO USE HUB,{0}";
        /// <summary>
        /// Turn Auto-use any device on this port on/off:    "AUTO USE PORT,<address>"
        /// </summary>
        public const string CommandAutoUsePort = "AUTO USE PORT,{0}";
        /// <summary>
        /// Turn Auto-use this device on any port on/off:    "AUTO USE DEVICE,<address>"
        /// </summary>
        public const string CommandAutoUseDevice = "AUTO USE DEVICE,{0}";
        /// <summary>
        /// Turn Auto-use this device on this port on/off:    "AUTO USE DEVICE PORT,<address>"
        /// </summary>
        public const string CommandAutoUseDevicePort = "AUTO USE DEVICE PORT,{0}";
        /// <summary>
        /// Clear all auto-use settings
        /// </summary>
        public const string CommandAutoUseClearAll = "AUTO USE CLEAR ALL\n";
        /// <summary>
        /// Specify server to connect to:    "MANUAL HUB ADD,<address>[:port] | <EasyFind address>"
        /// </summary>
        public const string CommandManualHubAdd = "MANUAL HUB ADD,{0}";
        /// <summary>
        /// Remove a manually specified hub:    "MANUAL HUB REMOVE,<address>[:port] | <EasyFind address>"
        /// </summary>
        public const string CommandManualHubRemove = "MANUAL HUB REMOVE,{0}";
        /// <summary>
        /// Remove all manually specified hubs
        /// </summary>
        public const string CommandManualHubRemoveAll = "MANUAL HUB REMOVE ALL\n";
        /// <summary>
        /// Add a reverse client to the server:    "ADD REVERSE,<server serial>,<client address[:port]>"
        /// </summary>
        public const string CommandAddReverse = "ADD REVERSE,{0},{1}";
        /// <summary>
        /// Remove a reverse client from the server:    "REMOVE REVERSE,<server serial>,<client address[:port]>"
        /// </summary>
        public const string CommandRemoveReverse = "REMOVE REVERSE,{0},{1}";
        /// <summary>
        /// List all reverse clients:    "LIST REVERSE,<server serial>"
        /// </summary>
        public const string CommandListReverse = "LIST REVERSE,{0}";
        /// <summary>
        /// List manually specified hubs
        /// </summary>
        public const string CommandManualHubList = "MANUAL HUB LIST\n";
        /// <summary>
        /// List licenses
        /// </summary>
        public const string CommandListLicenses = "LIST LICENSES\n";
        /// <summary>
        /// License server:    "LICENSE SERVER,<license key>"
        /// </summary>
        public const string CommandLicenseServer = "LICENSE SERVER,{0}";
        /// <summary>
        /// Clear client log
        /// </summary>
        public const string CommandClearLog = "CLEAR LOG\n";
        /// <summary>
        /// Set a custom device event:    "CUSTOM EVENT,<address>,<event>"
        /// </summary>
        public const string CommandCustomEvent = "CUSTOM EVENT,{0},{1}";
        /// <summary>
        /// Turn auto-find on
        /// </summary>
        public const string CommandAutoFind = "AUTOFIND\n";
        /// <summary>
        /// Turn reverse lookup on
        /// </summary>
        public const string CommandReverse = "REVERSE";
        /// <summary>
        /// Turn reverse SSL lookup on
        /// </summary>
        public const string CommandSslReverse = "SSLREVERSE";
        /// <summary>
        /// Shutdown the client
        /// </summary>
        public const string CommandExit = "EXIT";

        public const string ResultOk = "OK";
    }
}
