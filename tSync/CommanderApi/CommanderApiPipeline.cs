using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System.Collections.Generic;
using System.Threading.Channels;
using tSync.CommanderApi.Filters;
using tSync.CommanderApi.Models;
using tSync.CommanderApi.Options;
using tSync.Filters;
using tSync.Model;
using tSync.TwinzoApi;
using tUtils.Filters;

namespace tSync.CommanderApi
{
    internal class CommanderApiPipeline : Pipeline
    {
        private readonly ILogger logger;
        private readonly CommanderApiPipelineOptions opt;

        public CommanderApiPipeline(CommanderApiPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.CommanderApi);
        }

        public override void Register(ICollection<Filter> filters)
        {
            logger.LogTrace($"{GetType().Name} -> Register");
            logger.LogInformation(opt.ToString());

            // Setup Twinzo connection
            ConnectionOptionsBuilder optionsBuilder = new ConnectionOptionsBuilder();
            ConnectionOptions connectionOptions = optionsBuilder
                .Url(opt.Twinzo.TwinzoBaseUrl)
                .Client("Infotech")
                .ClientGuid(opt.Twinzo.ClientGuid.ToString())
                .BranchGuid(opt.Twinzo.BranchGuid.ToString())
                .ApiKey(opt.Twinzo.ApiKey)
                .Timeout(opt.Twinzo.Timeout)
                .Version(ConnectionOptions.VERSION_3)
                .Build();

            var connectorV3 = (DevkitConnectorV3)DevkitFactory.CreateDevkitConnector(connectionOptions);
            var memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = System.TimeSpan.FromSeconds(10)
            });
            var cacheConnector = new DevkitCacheConnector(connectorV3, memoryCache);
            cacheConnector.ExpirationInSeconds = opt.MemoryCache.ExpirationInSeconds;

            // Create channels
            Channel<CommanderPosition> gpsChannel;
            Channel<DeviceLocationContract> locationChannel;
            Channel<DeviceLocationContract> locationChannel2;

            if (opt.Channel.Capacity < 1)
            {
                gpsChannel = Channel.CreateUnbounded<CommanderPosition>();
                locationChannel = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel2 = Channel.CreateUnbounded<DeviceLocationContract>();
            }
            else
            {
                gpsChannel = Channel.CreateBounded<CommanderPosition>(opt.Channel.Capacity);
                locationChannel = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel2 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }

            // Create filters
            var pollingFilter = new CommanderApiPollingFilter(
                gpsChannel.Writer,
                opt.ApiBaseUrl,
                opt.Username,
                opt.Password,
                opt.PollIntervalMillis);

            var locationFilter = new CommanderLocationTransformFilter(
                gpsChannel.Reader,
                locationChannel.Writer,
                cacheConnector,
                opt.Twinzo.BranchGuid,
                opt.Twinzo.SectorId,
                opt.PollIntervalMillis);

            var areaFilter = new AreaFilter(locationChannel.Reader, locationChannel2.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel2.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            // Create vehicle name update filter
            var vehicleNameFilter = new CommanderVehicleNameFilter(
                opt.ApiBaseUrl,
                opt.Username,
                opt.Password,
                cacheConnector,
                opt.VehicleNameUpdateIntervalMillis);

            // Add filters to collection
            filters.Add(pollingFilter);
            filters.Add(locationFilter);
            filters.Add(areaFilter);
            filters.Add(rtlsFilter);
            filters.Add(vehicleNameFilter);

            // Set logger for all filters
            foreach (var filter in filters)
            {
                filter.Logger = logger;
            }
        }
    }
}
