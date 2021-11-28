using System.Collections.Generic;

namespace vrhero.VirtualHere
{
    public class ClientConfiguration
    {
        public GeneralSection General { get; set; }

        public SettingsSection Settings { get; set; }

        public Dictionary<string, bool> AutoShare { get; set; }

        public TransportSection Transport { get; set; }
    }

    public class GeneralSection
    {
        /// <summary>
        /// 自动查找共享器
        /// </summary>
        public bool? AutoFind { get; set; }

        /// <summary>
        /// 按服务器名称自动连接
        /// </summary>
        public bool? QualifyByName { get; set; }

        /// <summary>
        /// 按服务器网络接口自动接连
        /// </summary>
        public bool? QualifyByInterface { get; set; }

        /// <summary>
        /// 自动连接超时
        /// </summary>
        public int? AutoUseDelaySec { get; set; }

        /// <summary>
        /// 自动重连连接超时
        /// </summary>
        public int? RetryAutoUseDelaySec { get; set; }

        /// <summary>
        /// 查找新共享器的间隔
        /// </summary>
        public int? AutoRefreshLookupPeriod { get; set; }

        /// <summary>
        /// 查找新共享器的持续时间
        /// </summary>
        public int? BonjourLookupTimeout { get; set; }

        /// <summary>
        /// 等待共享器解析
        /// </summary>
        public int? BonjourResolverTimeout { get; set; }

        /// <summary>
        /// 客户端证书文件
        /// </summary>
        public string SSLClientCert { get; set; }

        /// <summary>
        /// 证书颁发机构文件
        /// </summary>
        public string SSLCAFile { get; set; }

        /// <summary>
        /// SSL Port
        /// </summary>
        public ushort? SSLPort { get; set; }

        /// <summary>
        /// 是否启用反向连接
        /// </summary>
        public bool? ReverseLookup { get; set; }

        /// <summary>
        /// 是否启用反向SSL连接
        /// </summary>
        public bool? SSLReverseLookup { get; set; }
    }

    public class TransportSection
    {
        public string EasyFindId { get; set; }

        public string EasyFindPin { get; set; }

        /// <summary>
        /// 服务器Ping周期，秒
        /// </summary>
        public int? PingInterval { get; set; }

        /// <summary>
        /// 服务器Ping超时，秒
        /// </summary>
        public int? PingTimeout { get; set; }

        /// <summary>
        /// 压缩阈值，bytes
        /// </summary>
        public int? CompressionLimit { get; set; }
    }

    public class SettingsSection
    {
        /// <summary>
        /// 手动添加共享器列表
        /// </summary>
        public string ManualHubs { get; set; }
    }
}
