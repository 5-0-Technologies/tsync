using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

//using MQTTnet.Client.Connecting;
//using MQTTnet.Client.Disconnecting;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using tUtils.Filters.Input;

namespace tSync.RFControls.Filters
{
    public class MqttFilter : InputChannelFilter<MqttApplicationMessage>
    {
        private readonly ManagedMqttClientOptions mqttOptions;
        private readonly string[] topics;
        private readonly IManagedMqttClient mqttClient;

        public MqttFilter(ChannelWriter<MqttApplicationMessage> channelWriter, ManagedMqttClientOptions mqttOptions, string[] topics) : base(channelWriter)
        {
            this.mqttOptions = mqttOptions ?? throw new ArgumentNullException(nameof(mqttOptions));
            this.topics = topics ?? throw new ArgumentNullException(nameof(topics));
            mqttClient = new MqttFactory().CreateManagedMqttClient();
            mqttClient.ApplicationMessageReceivedAsync += MessageHandler;
            mqttClient.ConnectedAsync += ConnectHandler;
            mqttClient.DisconnectedAsync += DisconnectHandler;
        }

        protected override async Task Run()
        {
            try
            {
                foreach (var topic in topics)
                {
                    await mqttClient.SubscribeAsync(new[] { new MqttTopicFilterBuilder().WithTopic(topic).Build() });
                }

                Logger.LogInformation($"{GetType().Name}: Client starting.");
                await mqttClient.StartAsync(mqttOptions);
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
                Logger.LogInformation($"{GetType().Name}: Client stopping.");
                await mqttClient.StopAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        private async Task MessageHandler(MqttApplicationMessageReceivedEventArgs message)
        {
            Logger.LogInformation($"{GetType().Name}: Message received.");
            await Writer.WriteAsync(message.ApplicationMessage);
        }

        private Task ConnectHandler(MqttClientConnectedEventArgs args)
        {
            Logger.LogInformation($"{GetType().Name}: Client connected.");
            return Task.CompletedTask;
        }

        private Task DisconnectHandler(MqttClientDisconnectedEventArgs args)
        {
            Logger.LogInformation($"{GetType().Name}: Client disconnected.");
            return Task.CompletedTask;
        }

        public override Task Loop()
        {
            return Task.CompletedTask;
        }
    }
}
