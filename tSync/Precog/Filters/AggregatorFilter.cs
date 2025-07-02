using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using tSync.Precog.Models;
using tUtils.Filters.Output;

namespace tSync.Precog.Filters
{
    public class AggregatorFilter : OutputChannelFilter<PrecogData>
    {
        private readonly IDictionary<long, AggregateData> channelWriter;
        private readonly int intervalMs;

        public AggregatorFilter(ChannelReader<PrecogData> channelReader, IDictionary<long, AggregateData> channelWriter, int intervalMs) : base(channelReader)
        {
            this.channelWriter = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
            this.intervalMs = intervalMs;
        }

        protected override void ProcessData(PrecogData precogData)
        {
            try
            {
                if (precogData is null || !precogData.EndTimeUtc.HasValue)
                {
                    return;
                }

                TimeSpan span = precogData.EndTimeUtc.Value - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var timestamp = (long)span.TotalMilliseconds;
                var interval = CeilTimestamp(timestamp, intervalMs);
                SaveToCache(interval, precogData.BeaconName, precogData.Mac, precogData.RSSI);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        public long CeilTimestamp(long timestamp, long intervalMs)
        {
            return (long)Math.Ceiling((double)(timestamp / intervalMs)) * intervalMs;
        }

        public void SaveToCache(long interval, string beaconName, string deviceName, short rssi)
        {
            if (!channelWriter.TryGetValue(interval, out AggregateData aggregateData))
            {
                aggregateData = new AggregateData()
                {
                    Timestamp = interval,
                };
                channelWriter.Add(interval, aggregateData);
            }

            if (!aggregateData.Devices.TryGetValue(deviceName, out Device device))
            {
                device = new Device()
                {
                    Name = deviceName,
                };
                aggregateData.Devices.Add(deviceName, device);
            };

            if (!device.Beacons.TryGetValue(beaconName, out Beacon beacon))
            {
                beacon = new Beacon()
                {
                    Name = beaconName,
                    RSSI = rssi,
                };
                device.Beacons.Add(beaconName, beacon);
            };

            beacon.RSSI = (beacon.RSSI + rssi) / 2;
        }
    }
}
