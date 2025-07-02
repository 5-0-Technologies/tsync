using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Channels;
using tSync.Filters;
using tSync.Model;
using tSync.Quuppa.Filters;
using tSync.Quuppa.Models;
using tSync.Quuppa.Options;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.Input;
using tUtils.Filters.InputOutput;
using System.Threading;
using MQTTnet.Formatter;

namespace tSync.Quuppa
{
    public class QuuppaPipeline : Pipeline
    {
        private readonly ILogger logger;
        private readonly QuuppaPipelineOptions opt;

        public QuuppaPipeline(QuuppaPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.Quuppa);
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
            Channel<QuuppaData> quuppaChannel;
            Channel<QuuppaLocationWrapper> locationChannel;
            Channel<QuuppaLocationWrapper> locationChannel2;
            Channel<DeviceLocationContract> locationChannel3;
            Channel<DeviceLocationContract> locationChannel4;
            if (opt.Channel.Capacity < 1)
            {
                channel = Channel.CreateUnbounded<byte[]>();
                quuppaChannel = Channel.CreateUnbounded<QuuppaData>();
                locationChannel = Channel.CreateUnbounded<QuuppaLocationWrapper>();
                locationChannel2 = Channel.CreateUnbounded<QuuppaLocationWrapper>();
                locationChannel3 = Channel.CreateUnbounded<DeviceLocationContract>();
                locationChannel4 = Channel.CreateUnbounded<DeviceLocationContract>();
            }
            else
            {
                channel = Channel.CreateBounded<byte[]>(opt.Channel.Capacity);
                quuppaChannel = Channel.CreateBounded<QuuppaData>(opt.Channel.Capacity);
                locationChannel = Channel.CreateBounded<QuuppaLocationWrapper>(opt.Channel.Capacity);
                locationChannel2 = Channel.CreateBounded<QuuppaLocationWrapper>(opt.Channel.Capacity);
                locationChannel3 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
                locationChannel4 = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }

            // Variables
            // Set up UDP listener only if udpOptions is not null
            if (opt.UdpOptions != null)
            {
                UdpClient udpClient = new UdpClient(opt.UdpOptions.Port);

                // Filters
                var udpFilter = new UdpListenerFilter(channel.Writer, udpClient);

                filters.Add(udpFilter);
            }

            // MQTT Setup only if mqttOptions is not null
            if (opt.MqttOptions != null)
            {
                var mqttFactory = new MqttFactory();
                var mqttClient = mqttFactory.CreateMqttClient();
                var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId($"QuuppaClient_{Guid.NewGuid()}")
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                    .WithCleanSession()
                    .WithTimeout(TimeSpan.FromSeconds(10));

                if (opt.MqttOptions.UseWebSockets)
                {
                    mqttClientOptionsBuilder.WithWebSocketServer(opt.MqttOptions.Host);
                }
                else
                {
                    mqttClientOptionsBuilder.WithTcpServer(opt.MqttOptions.Host, opt.MqttOptions.Port);
                }

                if (!string.IsNullOrEmpty(opt.MqttOptions.Username) &&
                    !string.IsNullOrEmpty(opt.MqttOptions.Password))
                {
                    mqttClientOptionsBuilder.WithCredentials(opt.MqttOptions.Username, opt.MqttOptions.Password);
                }

                mqttClientOptionsBuilder.WithProtocolVersion(MqttProtocolVersion.V500);

                var mqttClientOptions = mqttClientOptionsBuilder.Build();

                // Connect MQTT client
                mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None).Wait();

                var mqttFilter = new MqttListenerFilter(channel.Writer, mqttClient, opt.MqttOptions.Topic);
                filters.Add(mqttFilter);
            }

            // Filters
            var quuppaFilter = new QuuppaTransformFilter(channel.Reader, quuppaChannel.Writer);
            filters.Add(quuppaFilter);

            var locationFilter = new LocationTransformFilter(quuppaChannel.Reader, locationChannel.Writer, cacheConnector, opt.Twinzo.BranchGuid, opt.QuuppaScanIntervalMillis);
            var buttonFilter = new PanicButtonFilter(locationChannel.Reader, locationChannel2.Writer, cacheConnector);
            var transformFilter = new TransformChannelFilter<QuuppaLocationWrapper, DeviceLocationContract>(
                locationChannel2.Reader,
                locationChannel3.Writer,
                (qlw) =>
                {
                    return qlw.DeviceLocationContract;
                });
            var areaFilter = new AreaFilter(locationChannel3.Reader, locationChannel4.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel4.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            filters.Add(locationFilter);
            filters.Add(buttonFilter);
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
