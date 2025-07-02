using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using tSync.Filters;
using tSync.Model;
using tSync.Simulator.Filters;
using tSync.Simulator.Options;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.InputOutput;

namespace tSync.Simulator
{
    internal class SimulatorPipeline : Pipeline
    {
        private readonly ILogger logger;
        private readonly SimulatorPipelineOptions opt;

        public SimulatorPipeline(SimulatorPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.Simulator);
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
                ExpirationScanFrequency = TimeSpan.FromSeconds(10)
            });
            var cacheConnector = new DevkitCacheConnector(connectorV3, memoryCache);

            // Create channels
            Channel<DeviceLocationContract> locationChannel;
            Channel<DeviceLocationContract> locationChannel2;
            Channel<DeviceLocationContract> locationChannel3;

            if (opt.Channel.Capacity < 1)
            {
                locationChannel = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel2 = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel3 = Channel.CreateUnbounded<DeviceLocationContract>();
            }
            else
            {
                locationChannel = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel2 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel3 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }

            // Create filters
            var pathSimulator = new DevicePathSimulatorFilter(locationChannel.Writer, cacheConnector, opt.Paths, opt.RtlsSender.SendIntervalMillis);
            var timerFilter = new TimerFilter(pathSimulator, opt.UpdateInterval, 1, 1); // Update every second
            var locationTransformFilter = new SimulatorLocationTransformFilter(locationChannel.Reader, locationChannel2.Writer, cacheConnector, opt.Twinzo.BranchGuid);
            var areaFilter = new AreaFilter(locationChannel2.Reader, locationChannel3.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel3.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            // Add filters to collection
            filters.Add(timerFilter);
            filters.Add(pathSimulator);
            filters.Add(locationTransformFilter);
            filters.Add(areaFilter);
            filters.Add(rtlsFilter);

            // Set logger for all filters
            foreach (var filter in filters)
            {
                filter.Logger = logger;
            }
        }
    }
}
