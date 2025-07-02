using SDK;
using tSync.Model;

namespace tSync.TwinzoApi
{
    public partial class TwinzoApi
    {
        private const string ApiUrl = "https://twin.rtls.solutions/api/";
        private ConnectionOptionsBuilder optionsBuilder;
        public DevkitConnectorV3 devkitConnector { get; private set; }
        public TwinzoApi? BuildConnector(Tenant tenant)
        {
            // Define connection options with specific credentials and client identifiers
            optionsBuilder = new ConnectionOptionsBuilder();
            ConnectionOptions connectionOptions = optionsBuilder
                .Url(ApiUrl)
                .Client(tenant.TwinzoClientName)
                .ClientGuid(tenant.TwinzoClientGuid)
                .BranchGuid(tenant.TwinzoBranchGuid)
                .Timeout(1000)
                .ApiKey(tenant.TwinzoApiKey)
                .Version(ConnectionOptions.VERSION_3)
                .Build();

            // Create tDevKit connector class instance and authorize by credentials in builder
            devkitConnector = (DevkitConnectorV3)DevkitFactory.CreateDevkitConnector(connectionOptions);

            return this;
        }
    }
}
