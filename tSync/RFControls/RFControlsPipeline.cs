using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Filters;
using tSync.Model;
using tSync.RFControls.Filters;
using tSync.RFControls.Models;
using tSync.RFControls.Options;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.InputOutput;

namespace tSync.RFControls
{
    public class RFControlsPipeline : Pipeline
    {
        private readonly RFControlsPipelineOptions opt;
        private readonly ILogger logger;

        public RFControlsPipeline(RFControlsPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.RFControls);
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

            var mqttOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(opt.Mqtt.WithAutoReconnectDelay)
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Client1")
                    .WithTcpServer(opt.Mqtt.TcpServer, opt.Mqtt.Port)
                    .WithCredentials(opt.Mqtt.User, opt.Mqtt.Password)
                    .Build())
                .Build();

            const string tagBlink = "tagBlinkLite/";
            var topics = new string[opt.Regions.Length];
            for (var i = 0; i < opt.Regions.Length; i++)
            {
                if(Guid.TryParse(opt.Regions[i], out var region))
                {
                    topics[i] = tagBlink + region;
                }else
                {
                    logger.LogInformation("Region: {0} is not in XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX format. Skipped.", opt.Regions[i]);
                }            
            }

            // Channels
            Channel<MqttApplicationMessage> inputChannel;
            Channel<TagBlink> tagChannel;
            Channel<DeviceLocationContract> locationChannel;
            Channel<DeviceLocationContract> locationChannel2;
            if (opt.Channel.Capacity < 1)
            {
                inputChannel = Channel.CreateUnbounded<MqttApplicationMessage>();
                tagChannel = Channel.CreateUnbounded<TagBlink>();
                locationChannel = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel2 = Channel.CreateUnbounded<DeviceLocationContract>();
            }
            else
            {
                inputChannel = Channel.CreateBounded<MqttApplicationMessage>(opt.Channel.Capacity);
                tagChannel = Channel.CreateBounded<TagBlink>(opt.Channel.Capacity);
                locationChannel = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel2 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }

            var mqttFilter = new MqttFilter(inputChannel, mqttOptions, topics);
            var transformFilter = new TransformChannelFilter<MqttApplicationMessage, TagBlink>(inputChannel.Reader, tagChannel.Writer, TransformChannel);
            var locationFilter = new LocationTransformFilter(tagChannel.Reader, locationChannel.Writer, cacheConnector, opt.Twinzo.BranchGuid, opt.ReceiveInterval);
            var areaFilter = new AreaFilter(locationChannel.Reader, locationChannel2.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel2.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            filters.Add(mqttFilter);
            filters.Add(transformFilter);
            filters.Add(locationFilter);
            filters.Add(areaFilter);
            filters.Add(rtlsFilter);

            foreach (var filter in filters)
            {
                filter.Logger = logger;
            }
        }

        private async Task TransformChannel(ChannelReader<MqttApplicationMessage> reader, ChannelWriter<TagBlink> writer, CancellationToken ct)
        {
            try
            {
                var mqttData = await reader.ReadAsync(ct);
                var tagBlinks = JsonSerializer.Deserialize<TagBlink[]>(mqttData.Payload);
                if (tagBlinks != null){
                    foreach (var tagBlink in tagBlinks)
                    {
                        await writer.WriteAsync(tagBlink, ct);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
            }
        }
    }
}
