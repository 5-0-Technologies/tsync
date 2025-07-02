using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Cisco.Models;
using tUtils.Filters;

namespace tSync.Cisco.Filters
{
    public class CiscoTransformFilter : ChannelFilter<byte[], CiscoData>
    {
        public CiscoTransformFilter(ChannelReader<byte[]> channelReader, ChannelWriter<CiscoData> channelWriter) : base(channelReader, channelWriter)
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
                var bytes = await Reader.ReadAsync();
                var ciscoData = JsonSerializer.Deserialize<CiscoData>(bytes, new JsonSerializerOptions());
                Logger.LogTrace(ciscoData.ToString());
                await Writer.WriteAsync(ciscoData);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, $"{GetType().Name}: Error transforming Cisco data");
            }
        }
    }
} 