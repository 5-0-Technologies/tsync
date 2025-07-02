using Core.Enum;
using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Quuppa.Models;
using tSync.TwinzoApi;
using tUtils;
using tUtils.Filters;

namespace tSync.Quuppa.Filters
{
    public class PanicButtonFilter : ChannelFilter<QuuppaLocationWrapper, QuuppaLocationWrapper>
    {
        private readonly ConcurrentDictionary<string, ButtonState?> buttonStates;
        private readonly DevkitCacheConnector connector;

        public PanicButtonFilter(ChannelReader<QuuppaLocationWrapper> channelReader, ChannelWriter<QuuppaLocationWrapper> channelWriter,
            DevkitCacheConnector connector) : base(channelReader, channelWriter)
        {
            if (channelReader is null)
            {
                throw new ArgumentNullException(nameof(channelReader));
            }

            if (channelWriter is null)
            {
                throw new ArgumentNullException(nameof(channelWriter));
            }

            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
            buttonStates = new ConcurrentDictionary<string, ButtonState?>();
        }

        public override async Task Loop()
        {
            try
            {
                var qlWrapper = await Reader.ReadAsync(cancellationTokenSource.Token);

                if (IsPressed(qlWrapper.QuuppaData))
                {
                    _ = SendManDown(qlWrapper.QuuppaData);
                }

                await Writer.WriteAsync(qlWrapper, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        protected bool IsPressed(QuuppaData quuppaData)
        {
            Logger.LogTrace($"{GetType().Name}: IsPressed");
            bool isPressed;
            if (!buttonStates.TryGetValue(quuppaData.TagId, out var lastButtonState))
            {
                isPressed = quuppaData.Button1State == ButtonState.Pushed;
            }
            else
            {
                isPressed = lastButtonState != ButtonState.Pushed && quuppaData.Button1State == ButtonState.Pushed;
            }
            buttonStates[quuppaData.TagId] = quuppaData.Button1State;

            Logger.LogTrace($"Tag: {quuppaData.TagName} -> Button1Pressed: {isPressed}");
            return isPressed;
        }

        protected async Task SendManDown(QuuppaData quuppaData)
        {
            try
            {
                Logger.LogTrace($"{GetType().Name}: SendManDown");
                var response = await connector.ManDownBatch(new ManDownContract[] {
                    new ManDownContract()
                    {
                        Login = quuppaData.TagId,
                        FallType = FallType.ManDownPositive,
                        Timestamp = quuppaData.Button1StateTS.HasValue ? quuppaData.Button1StateTS.Value : DateTime.UtcNow.ToUnixTimestamp()
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }
    }
}
