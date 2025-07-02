using tSync.RtlsDataSource;

namespace tSync.Model
{
    public class Tenant
    {
        public string TwinzoClientName { get; set; }
        public string TwinzoClientGuid { get; set; }
        public string TwinzoBranchGuid { get; set; }
        public string TwinzoApiKey { get; set; }
        public string RtlsType { get; set; }
        public string RtlsConnectionString { get; set; }
        public IRtlsDataSource RtlsDataSource { get; private set; }

        public void BuildRtlsDataSource()
        {
            RtlsDataSource =
            RtlsType switch
            {
                "quuppa" => new QuuppaUdpDataSource(),
                "simatic_udp" => new SimaticRtlsDataSource(),
                "spin" => new SpinPgsqlDataSource(),
                _ => null,
            };
        }
    }
}