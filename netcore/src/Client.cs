using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace vrhero.VirtualHere
{
    public class VirtualHereClient : IDisposable
    {
        private readonly TimeSpan defaultQueryCacheExpired = TimeSpan.FromSeconds(5);
        private readonly TimeSpan defaultExecCacheExpired = TimeSpan.FromSeconds(1);
        private readonly int defaultTimeout = 5000;
        private readonly int _timeout;
        private readonly IClient _client;

        public int Timeout => _timeout;

        public bool AutoFindHubs { get; set; }

        public bool AutoUseAllDevices { get; set; }

        public VirtualHereClient(int timeout = 0)
        {
            _timeout = timeout < 1 ? defaultTimeout : timeout;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _client = new WindowsClient(_timeout);
            }
            else
            {
                _client = new NonWindowsClient(_timeout);
            }
        }

        private string ParseBytesString(string value)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var buf = Convert.FromBase64String(value);
                if (!buf.All(c => c.Equals(0)))
                {
                    result = BitConverter.ToString(buf).Replace("-", string.Empty).ToLower();
                }
            }
            return result;
        }

        private string ParseIpAddress(string value)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var buf = Convert.FromBase64String(value);
                if (buf.Any(c => !c.Equals(0)))
                {
                    result = new System.Net.IPAddress(buf).ToString();
                }
            }
            return result;
        }

        public async Task<string> ExecCommand(string command)
        {
            return await _client.QueryAsync(command);
        }

        public async Task<ServerInfo[]> List()
        {
            var cacheKey = "VirtualHereClient:List";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out List<ServerInfo> result))
            {
                return result.ToArray();
            }
            else
            {
                var servers = new List<ServerInfo>();
                var buffer = await _client.QueryAsync(Constants.CommandList);
                if (!string.IsNullOrWhiteSpace(buffer))
                {
                    ServerInfo server = null;
                    var lines = buffer.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 2; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                        {
                            continue;
                        }
                        var match = Regex.Match(lines[i].Trim(), @"(.+)\((.+):(\d+)\)");
                        if (match.Success && match.Groups.Count > 3)
                        {
                            server = new ServerInfo()
                            {
                                Name = match.Groups[1].Value,
                                Address = match.Groups[2].Value,
                                Devices = new List<DeviceInfo>()
                            };
                            if (int.TryParse(match.Groups[3].Value, out int port))
                            {
                                server.Port = port;
                            }
                            servers.Add(server);
                        }
                        else
                        {
                            match = Regex.Match(lines[i], @"\s+\-\-\>\s+(.+)\((.+)\.(\d+)\)");
                            if (match.Success && match.Groups.Count > 3)
                            {
                                if (match.Groups[1].Value.Trim() == "0xec00")
                                {
                                    continue;
                                }
                                if (server != null)
                                {
                                    var device = new DeviceInfo() { Vendor = match.Groups[1].Value, Address = $"{match.Groups[2].Value}.{match.Groups[3].Value}" };
                                    server.Devices.Add(device);
                                }
                            }
                            else
                            {

                                if (lines[i].StartsWith("Auto-Find currently"))
                                {
                                    AutoFindHubs = lines[i].Substring(20).Trim() == "on";
                                }
                                else if (lines[i].StartsWith("Auto-Use All currently"))
                                {
                                    AutoUseAllDevices = lines[i].Substring(22).Trim() == "on";
                                }
                                else if (lines[i].StartsWith("Reverse Lookup currently"))
                                {
                                    var ReverseLookup = lines[i].Substring(25).Trim() == "on";
                                }
                                else if (lines[i].StartsWith("Reverse SSL Lookup currently"))
                                {
                                    var ReverseSslLookup = lines[i].Substring(29).Trim() == "on";
                                }
                                else if (lines[i].StartsWith("VirtualHere Client is running as a service"))
                                {

                                }
                            }
                        }
                    }
                }
                result = new List<ServerInfo>();
                foreach (var item in servers)
                {
                    var server = await GetServerInfo(item.Name);
                    server.Devices = new List<DeviceInfo>();
                    foreach (var deviceItem in item.Devices)
                    {
                        var device = await GetDeviceInfo(deviceItem.Address);
                        server.Devices.Add(device);
                    }
                    result.Add(server);
                }
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                return result.ToArray();
            }
        }

        public async Task<ClientState> GetClientState()
        {
            var cacheKey = "VirtualHereClient:GetClientState";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out ClientState result))
            {
                return result;
            }
            else
            {
                try
                {
                    var buffer = await _client.QueryAsync(Constants.CommandGetClientState);
                    using (var reader = new StringReader(buffer))
                    {
                        var serializer = new XmlSerializer(typeof(ClientState));
                        result = serializer.Deserialize(reader) as ClientState;
                        if (result != null)
                        {
                            foreach (var server in result.Servers)
                            {
                                if (server.Connection != null)
                                {
                                    server.Connection.UUID = ParseBytesString(server.Connection.UUID);
                                    server.Connection.TransportId = ParseBytesString(server.Connection.TransportId);
                                    server.Connection.EasyFindId = ParseBytesString(server.Connection.EasyFindId);
                                    server.Connection.EasyFindPin = ParseBytesString(server.Connection.EasyFindPin);
                                    //result.Server.Connection.ConnectedTime = new DateTimeOffset(result.Server.Connection.ConnectedTime, TimeSpan.FromHours(8)).LocalDateTime;
                                }
                                foreach (var device in server.Devices)
                                {
                                    device.ConnectionUUID = ParseBytesString(device.ConnectionUUID);
                                    device.BoundConnectionUUID = ParseBytesString(device.BoundConnectionUUID);
                                    device.BoundConnectionIp = ParseIpAddress(device.BoundConnectionIp);
                                    device.BoundConnectionIp6 = ParseIpAddress(device.BoundConnectionIp6);
                                }
                            }
                        }
                        MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                        return result;
                    }
                }
                catch
                {
                    return default;
                }
            }
        }

        public async Task<(bool, string)> UseDevice(string address, string password = null)
        {
            var cacheKey = $"VirtualHereClient:UseDevice:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandUseDevice,
                    string.IsNullOrEmpty(password) ? address : $"{address},{password}"));
                var ok = buffer == Constants.ResultOk;
                CleanCaches(ok);
                result = (ok, ok ? string.Empty : buffer); ;
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        private void CleanCaches(bool ok)
        {
            if (ok)
            {
                MemoryCacheManager.Default.Remove("VirtualHereClient:GetClientState");
                MemoryCacheManager.Default.Remove("VirtualHereClient:List");
            }
        }

        public async Task<(bool, string)> StopUsing(string address)
        {
            var cacheKey = $"VirtualHereClient:StopUsing:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandStopUsing, address));
                var ok = buffer == Constants.ResultOk;
                CleanCaches(ok);
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> StopUsingAll()
        {
            var cacheKey = "VirtualHereClient:StopUsing";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandStopUsingAll);
                var ok = buffer == Constants.ResultOk;
                CleanCaches(ok);
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> StopUsingAllLocal()
        {
            var cacheKey = "VirtualHereClient:StopUsingAllLocal";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandStopUsingAllLocal);
                var ok = buffer == Constants.ResultOk;
                CleanCaches(ok);
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<DeviceInfo> GetDeviceInfo(string address)
        {
            var cacheKey = $"VirtualHereClient:GetDeviceInfo:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out DeviceInfo result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandDeviceInfo, address));
                if (!string.IsNullOrWhiteSpace(buffer))
                {
                    result = new DeviceInfo();
                    foreach (var line in buffer.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            switch (parts[0].Trim())
                            {
                                case "ADDRESS":
                                    result.Address = parts[1].Trim();
                                    break;
                                case "VENDOR":
                                    result.Vendor = parts[1].Trim();
                                    break;
                                case "VENDOR ID":
                                    if (int.TryParse(parts[1].Replace("0x", string.Empty).Trim(), NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture, out int value))
                                    {
                                        result.VendorId = value;
                                    }
                                    break;
                                case "PRODUCT":
                                    result.Product = parts[1].Trim();
                                    break;
                                case "PRODUCT ID":
                                    if (int.TryParse(parts[1].Replace("0x", string.Empty).Trim(), NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture, out value))
                                    {
                                        result.ProductId = value;
                                    }
                                    break;
                                case "SERIAL":
                                    result.SerialNumber = parts[1].Trim();
                                    break;
                                case "IN USE BY":
                                    if (parts[1].Trim() == "YOU")
                                    {
                                        result.IsConnected = true;
                                    }
                                    else
                                    {
                                        var match = Regex.Match(parts[1].Trim(), @"(.+)\((.+)\)\s*AT\s*(.+)\s*\((.+)\)");
                                        if (match.Success && match.Groups.Count > 4)
                                        {
                                            result.InUseBy = new UseInfo()
                                            {
                                                Username = match.Groups[1].Value.Trim(),
                                                UserId = match.Groups[2].Value.Trim(),
                                                Address = match.Groups[3].Value.Trim(),
                                                Hostname = match.Groups[4].Value.Trim()
                                            };
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                return result;
            }
        }

        public async Task<ServerInfo> GetServerInfo(string serverName)
        {
            var cacheKey = $"VirtualHereClient:GetServerInfo:{serverName}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out ServerInfo result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandServerInfo, serverName));
                if (!string.IsNullOrWhiteSpace(buffer))
                {
                    result = new ServerInfo();
                    foreach (var line in buffer.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            switch (parts[0].Trim())
                            {
                                case "NAME":
                                    result.Name = parts[1].Trim();
                                    break;
                                case "VERSION":
                                    result.Version = parts[1].Trim();
                                    break;
                                case "STATE":
                                    result.State = parts[1].Trim();
                                    break;
                                case "ADDRESS":
                                    result.Address = parts[1].Trim();
                                    break;
                                case "PORT":
                                    if (int.TryParse(parts[1].Trim(), out int value))
                                    {
                                        result.Port = value;
                                    }
                                    break;
                                case "CONNECTED FOR":
                                    if (int.TryParse(parts[1].Replace("sec", string.Empty).Trim(), out value))
                                    {
                                        result.ConnectedFor = value;
                                    }
                                    break;
                                case "MAX DEVICES":
                                    if (int.TryParse(parts[1].Trim(), out value))
                                    {
                                        result.MaxDevices = value;
                                    }
                                    break;
                                case "CONNECTION ID":
                                    if (int.TryParse(parts[1].Trim(), out value))
                                    {
                                        result.ConnectedId = value;
                                    }
                                    break;
                                case "INTERFACE":
                                    result.Interface = parts[1].Trim();
                                    break;
                                case "SERIAL NUMBER":
                                    result.SerialNumber = parts[1].Trim();
                                    break;
                                case "EASYFIND":
                                    result.EasyFind = parts[1].Trim() != "not enabled";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> DeviceRename(string address, string nickname)
        {
            var cacheKey = $"VirtualHereClient:ServerRename:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandDeviceRename, address, nickname));
                var ok = buffer == Constants.ResultOk;
                CleanCaches(ok);
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ServerRename(string name, string address, ushort port = 7575)
        {
            var cacheKey = $"VirtualHereClient:ServerRename:{address}:{port}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandServerRename, $"{address}:{port}", name));
                var ok = buffer == Constants.ResultOk;
                CleanCaches(ok);
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoUseHub(string serverName)
        {
            var cacheKey = $"VirtualHereClient:AutoUseHub:{serverName}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandAutoUseHub, serverName));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoUsePort(string address)
        {
            var cacheKey = $"VirtualHereClient:AutoUsePort:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandAutoUsePort, address));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoUseDevice(string address)
        {
            var cacheKey = $"VirtualHereClient:AutoUseDevice:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandAutoUseDevice, address));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoUseDevicePort(string address)
        {
            var cacheKey = $"VirtualHereClient:AutoUseDevicePort:{address}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandAutoUseDevicePort, address));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoUseAll()
        {
            var cacheKey = "VirtualHereClient:AutoUseAll";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandAutoUseAll);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoUseClearAll()
        {
            var cacheKey = "VirtualHereClient:AutoUseClearAll";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandAutoUseClearAll);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ManualHubAdd(string address, ushort port = 7575)
        {
            var cacheKey = $"VirtualHereClient:ManualHubAdd:{address}:{port}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandManualHubAdd, $"{address}:{port}"));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ManualHubAdd(string easyFindAddress)
        {
            var cacheKey = $"VirtualHereClient:ManualHubAdd:{easyFindAddress}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandManualHubAdd, easyFindAddress));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ManualHubRemove(string address, ushort port = 7575)
        {
            var cacheKey = $"VirtualHereClient:ManualHubRemove:{address}:{port}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandManualHubRemove, $"{address}:{port}"));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ManualHubRemove(string easyFindAddress)
        {
            var cacheKey = $"VirtualHereClient:ManualHubRemove:{easyFindAddress}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandManualHubRemove, easyFindAddress));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ManualHubRemoveAll()
        {
            var cacheKey = "VirtualHereClient:ManualHubRemoveAll";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandManualHubRemoveAll);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AddReverse(string serverSerial, string clientAddress, ushort port = 7575)
        {
            var cacheKey = $"VirtualHereClient:ManualHubRemove:{serverSerial}:{clientAddress}:{port}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandAddReverse, serverSerial, $"{clientAddress}:{port}"));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> RemoveReverse(string serverSerial, string clientAddress, ushort port = 7575)
        {
            var cacheKey = $"VirtualHereClient:RemoveReverse:{serverSerial}:{clientAddress}:{port}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandRemoveReverse, serverSerial, $"{clientAddress}:{port}"));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<string[]> ListReverse(string serverSerial)
        {
            var cacheKey = $"VirtualHereClient:ListReverse:{serverSerial}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out string[] result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandListReverse, serverSerial));
                result = buffer.Trim().Split(new char[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                return result;
            }
        }

        public async Task<string[]> ManualHubList()
        {
            var cacheKey = "VirtualHereClient:ManualHubList";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out string[] result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandManualHubList);
                result = buffer.Trim().Split(new char[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                return result;
            }
        }

        public async Task<(LicenseInfo, string)> ListLicenses()
        {
            var cacheKey = "VirtualHereClient:ListLicenses";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (LicenseInfo, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandListLicenses);
                var parts = buffer.Trim().Split(',');
                if (parts.Length > 2 && parts[1].StartsWith("s/n="))
                {
                    var licenseInfo = new LicenseInfo();
                    licenseInfo.SerialNumber = parts[1].Substring(4);
                    var devices = parts[2].Split(' ');
                    if (devices.Length > 1)
                    {
                        licenseInfo.IsUnlimited = devices[0] == "unlimited";
                        if (!licenseInfo.IsUnlimited)
                        {
                            licenseInfo.IsUnregistered = devices[0] == "1";
                            if (!licenseInfo.IsUnregistered && int.TryParse(devices[0], out int count))
                            {
                                licenseInfo.MaxDeviceCount = count;
                            }
                        }
                    }
                    result = (licenseInfo, buffer);
                    MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultQueryCacheExpired);
                }
                return result;
            }
        }

        public async Task<(bool, string)> LicenseServer(string licenseKey)
        {
            var cacheKey = $"VirtualHereClient:LicenseServer:{licenseKey}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandLicenseServer, licenseKey));
                var ok = buffer == Constants.ResultOk;
                if (ok)
                {
                    MemoryCacheManager.Default.Remove("VirtualHereClient:ListLicenses");
                }
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> ClearLog()
        {
            var cacheKey = "VirtualHereClient:ClearLog";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandClearLog);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> CustomEvent(string address, string eventName)
        {
            var cacheKey = $"VirtualHereClient:CustomEvent:{address}:{eventName}";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(string.Format(Constants.CommandCustomEvent, address, eventName));
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> AutoFind()
        {
            var cacheKey = "VirtualHereClient:AutoFind";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandAutoFind);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> Reverse()
        {
            var cacheKey = "VirtualHereClient:Reverse";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandReverse);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> SslReverse()
        {
            var cacheKey = "VirtualHereClient:SslReverse";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandSslReverse);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public async Task<(bool, string)> Exit()
        {
            var cacheKey = "VirtualHereClient:SslReverse";
            if (MemoryCacheManager.Default.TryGet(cacheKey, out (bool, string) result))
            {
                return result;
            }
            else
            {
                var buffer = await _client.QueryAsync(Constants.CommandExit);
                var ok = buffer == Constants.ResultOk;
                result = (ok, ok ? string.Empty : buffer);
                MemoryCacheManager.Default.SetAbsoluteExpire(cacheKey, result, defaultExecCacheExpired);
                return result;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
