using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Filters;
using tSync.Model;
using tSync.ThingPark.Filters;
using tSync.ThingPark.Models;
using tSync.ThingPark.Options;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.Input;
using tUtils.Filters.InputOutput;

namespace tSync.ThingPark
{
    internal class ThingParkPipeline : Pipeline
    {
        private readonly ILogger logger;
        private readonly ThingParkPipelineOptions opt;

        public ThingParkPipeline(ThingParkPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.ThingPark);
        }

        public override void Register(ICollection<Filter> filters)
        {
            logger.LogTrace($"{GetType().Name} -> Register");
            logger.LogInformation(opt.ToString());

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
            cacheConnector.ExpirationInSeconds = opt.MemoryCache.ExpirationInSeconds;

            // Channels
            Channel<ThingParkData> tcpChannel;
            Channel<ThingParkData> thingParkChannel;
            Channel<ThingParkLocationWrapper> locationChannel;
            Channel<ThingParkLocationWrapper> locationChannel2;
            Channel<DeviceLocationContract> locationChannel3;
            Channel<DeviceLocationContract> locationChannel4;
            if (opt.Channel.Capacity < 1)
            {
                tcpChannel = Channel.CreateUnbounded<ThingParkData>();
                thingParkChannel = Channel.CreateUnbounded<ThingParkData>();
                locationChannel = Channel.CreateUnbounded<ThingParkLocationWrapper>();
                locationChannel2 = Channel.CreateUnbounded<ThingParkLocationWrapper>();
                locationChannel3 = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel4 = Channel.CreateUnbounded<DeviceLocationContract>();
            }

            else
            {
                tcpChannel = Channel.CreateBounded<ThingParkData>(opt.Channel.Capacity);
                thingParkChannel = Channel.CreateBounded<ThingParkData>(opt.Channel.Capacity);
                locationChannel = Channel.CreateBounded<ThingParkLocationWrapper>(opt.Channel.Capacity);
                locationChannel2 = Channel.CreateBounded<ThingParkLocationWrapper>(opt.Channel.Capacity);
                locationChannel3 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel4 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }

            // Filters
            var httpServer = new HttpServer(opt.HttpServer.Prefixes, logger);

            httpServer.Start();

            var tcpFilter = new HttpListenerFilter(tcpChannel.Writer, httpServer, cacheConnector);
            var thingParkFilter = new ThingParkTransformFilter(tcpChannel.Reader, thingParkChannel.Writer);

            var locationFilter = new ThingParkLocationTransformFilter(thingParkChannel.Reader, locationChannel.Writer, cacheConnector, opt.Twinzo.BranchGuid, opt.Twinzo.SectorId);
            var transformFilter = new TransformChannelFilter<ThingParkLocationWrapper, DeviceLocationContract>(
                locationChannel.Reader,
                locationChannel3.Writer,
                (qlw) =>
                {
                    return qlw.DeviceLocationContract;
                });
            var areaFilter = new AreaFilter(locationChannel3.Reader, locationChannel4.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel4.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            filters.Add(tcpFilter);
            filters.Add(thingParkFilter);
            filters.Add(locationFilter);
            filters.Add(transformFilter);
            filters.Add(areaFilter);
            filters.Add(rtlsFilter);

            foreach (var filter in filters)
            {
                filter.Logger = logger;
            }
        }
    }

}
