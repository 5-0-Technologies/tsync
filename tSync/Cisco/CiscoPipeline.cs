using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using tSync.Cisco.Filters;
using tSync.Cisco.Models;
using tSync.Cisco.Options;
using tSync.Filters;
using tSync.Model;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.InputOutput;

namespace tSync.Cisco
{
    public class CiscoPipeline : Pipeline
    {
        private readonly ILogger logger;
        private readonly CiscoPipelineOptions opt;

        public CiscoPipeline(CiscoPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.Cisco);
        }
         
        public override void Register(ICollection<Filter> filters)
        {
            logger.LogTrace($"{GetType().Name} -> Register");
            logger.LogInformation(opt.ToString());

            // Define your channels
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
            Channel<byte[]> channel;
            Channel<CiscoData> ciscoChannel;
            Channel<CiscoLocationWrapper> locationChannel;
            Channel<CiscoLocationWrapper> locationChannel2;
            Channel<DeviceLocationContract> locationChannel3;
            Channel<DeviceLocationContract> locationChannel4;
            
            if (opt.Channel.Capacity < 1)
            {
                channel = Channel.CreateUnbounded<byte[]>();
                ciscoChannel = Channel.CreateUnbounded<CiscoData>();
                locationChannel = Channel.CreateUnbounded<CiscoLocationWrapper>();
                locationChannel2 = Channel.CreateUnbounded<CiscoLocationWrapper>();
                locationChannel3 = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel4 = Channel.CreateUnbounded<DeviceLocationContract>();
            }
            else
            {
                channel = Channel.CreateBounded<byte[]>(opt.Channel.Capacity);
                ciscoChannel = Channel.CreateBounded<CiscoData>(opt.Channel.Capacity);
                locationChannel = Channel.CreateBounded<CiscoLocationWrapper>(opt.Channel.Capacity);
                locationChannel2 = Channel.CreateBounded<CiscoLocationWrapper>(opt.Channel.Capacity);
                locationChannel3 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel4 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }

            // HTTP Stream Setup
            if (opt.HttpStreamOptions != null)
            {
                var httpStreamFilter = new HttpStreamListenerFilter(channel.Writer, opt.HttpStreamOptions);
                filters.Add(httpStreamFilter);
            }

            // Filters
            var ciscoFilter = new CiscoTransformFilter(channel.Reader, ciscoChannel.Writer);
            filters.Add(ciscoFilter);

            var locationFilter = new LocationTransformFilter(ciscoChannel.Reader, locationChannel.Writer, cacheConnector, opt.Twinzo.BranchGuid);
            var transformFilter = new TransformChannelFilter<CiscoLocationWrapper, DeviceLocationContract>(
                locationChannel.Reader,
                locationChannel3.Writer,
                (clw) =>
                {
                    return clw.DeviceLocationContract;
                });
            var areaFilter = new AreaFilter(locationChannel3.Reader, locationChannel4.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel4.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

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