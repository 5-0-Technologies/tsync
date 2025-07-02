using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using tUtils.Filters.Output;

namespace tSync.Filters
{
    public class RtlsSenderFilter : OutputChannelFilter<DeviceLocationContract>
    {
        private readonly object dataLock = new object();
        private readonly ConcurrentDictionary<string, DeviceLocationContract> data;
        private readonly DevkitConnectorV3 Connector;
        private readonly SemaphoreSlim methodLock;
        private readonly System.Timers.Timer timer;
        private readonly int maxSize;

        public RtlsSenderFilter(ChannelReader<DeviceLocationContract> channelReader, DevkitConnectorV3 connector, double intervalMillis, int maxSize) : base(channelReader)
        {
            data = new ConcurrentDictionary<string, DeviceLocationContract>();
            Connector = connector ?? throw new ArgumentNullException(nameof(connector));

            methodLock = new SemaphoreSlim(1, 1);
            timer = new System.Timers.Timer(intervalMillis);
            timer.Elapsed += OnTimedEvent;
            this.maxSize = maxSize;
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            await SendToServer();
        }

        protected override async void ProcessData(DeviceLocationContract read)
        {
            lock (dataLock)
            {
                DeviceLocationContract deviceLocation;
                if (data.TryGetValue(read.Login, out deviceLocation))
                {
                    deviceLocation.Locations = deviceLocation.Locations.Concat(read.Locations).ToArray();
                }
                else
                {
                    data.TryAdd(read.Login, read);
                }
            }

            if (ShouldSend())
            {
                await SendToServer();
            }
        }

        private bool ShouldSend()
        {
            methodLock.Wait();
            try
            {
                var count = data.Values.SelectMany(k => k.Locations).Count();
                return count >= maxSize;
            }
            finally
            {
                methodLock.Release();
            }
        }

        private async Task SendToServer()
        {
            if (methodLock.CurrentCount == 0)
            {
                return;
            }

            await methodLock.WaitAsync();
            try
            {
                DeviceLocationContract[] requestData;
                lock (dataLock)
                {
                    requestData = data.Values.ToArray();
                    data.Clear();
                }
                if (requestData.Length > 0)
                {
                    Logger.LogTrace($"{GetType().Name}: Sending rtls data.");
                    var response = await Connector.AddLocalizationData(requestData);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "");
            }
            finally
            {
                methodLock.Release();
            }
        }

        protected override void BeforeRun()
        {
            timer.Start();
        }

        protected override void AfterRun()
        {
            timer.Stop();
        }
    }
}
