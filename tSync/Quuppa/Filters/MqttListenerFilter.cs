using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using tUtils.Filters.Input;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using MQTTnet;

namespace tSync.Quuppa.Filters
{
    public class MqttListenerFilter : InputChannelFilter<byte[]>
    {
        private readonly IMqttClient _mqttClient;
        private readonly string _topic;

        public MqttListenerFilter(ChannelWriter<byte[]> channelWriter, IMqttClient mqttClient, string topic)
            : base(channelWriter)
        {
            _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        private async Task HandleDisconnected(MqttClientDisconnectedEventArgs e)
        {
            base.Logger.LogWarning("MQTT disconnected. Attempting to reconnect...");
            await Task.Delay(5000);
            try
            {
                await _mqttClient.ReconnectAsync();
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_topic).Build());
                base.Logger.LogInformation($"Subscribed to MQTT topic: {_topic}");
            }
            catch (Exception ex)
            {
                base.Logger.LogError($"Reconnection failed: {ex.Message}");
            }
        }

        private async Task HandleMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                if (e.ApplicationMessage.PayloadSegment.Array != null)
                {
                    base.Logger.LogTrace($"MQTT message received: {Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment.Array)}");
                    await base.Writer.WriteAsync(e.ApplicationMessage.PayloadSegment.Array, cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                base.Logger.LogError($"Error processing MQTT message: {ex.Message}");
            }
        }

        public override async Task Loop()
        {
            try
            {
                // Set up event handlers
                _mqttClient.DisconnectedAsync += HandleDisconnected;
                _mqttClient.ApplicationMessageReceivedAsync += HandleMessage;

                // Subscribe to the MQTT topic
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_topic).Build());
                base.Logger.LogInformation($"Subscribed to MQTT topic: {_topic}");

                // Keep the filter alive and monitor connection
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                base.Logger.LogInformation("MQTT Listener stopped.");
            }
            catch (Exception ex)
            {
                base.Logger.LogError($"MQTT Listener error: {ex.Message}");
                throw;
            }
            finally
            {
                // Clean up event handlers
                _mqttClient.DisconnectedAsync -= HandleDisconnected;
                _mqttClient.ApplicationMessageReceivedAsync -= HandleMessage;
            }
        }
    }
}
