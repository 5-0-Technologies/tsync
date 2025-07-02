using Microsoft.Extensions.Logging;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Precog.Models;
using tUtils.Filters;

namespace tSync.Precog.Filters
{
    public class Transformator2Filter : ChannelFilter<PrecogBeacon, PrecogData>
    {

        public Transformator2Filter(ChannelReader<PrecogBeacon> channelReader, ChannelWriter<PrecogData> channelWriter) : base(channelReader, channelWriter)
        {
            if (channelReader is null)
            {
                throw new ArgumentNullException(nameof(channelReader));
            }

            if (channelWriter is null)
            {
                throw new ArgumentNullException(nameof(channelWriter));
            }
        }

        public override async Task Loop()
        {
            try
            {
                PrecogBeacon precogBeacon = await Reader.ReadAsync();
                if (precogBeacon is null)
                {
                    return;
                }

                foreach (var precogDevice in precogBeacon.PrecogDevices)
                {
                    var data = new PrecogData()
                    {
                        BeaconName = precogBeacon.Name,
                        AP_CountryId = precogDevice.CountryCode,
                        Mac = precogDevice.Mac,
                        FrameType = (byte)precogDevice.FrameType,
                        Channel = precogDevice.Channel,
                        SSID = precogDevice.SSID,
                        RSSI = precogDevice.RSSI,
                        AP_Mac = precogDevice.ConnMac,
                        StartTimeUtc = precogDevice.StartTimestamp,
                        EndTimeUtc = precogDevice.EndTimestamp,
                        DwellTime = precogDevice.DwellTimestamp,
                    };

                    await Writer.WriteAsync(data);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "");
            }
        }
    }
}
