using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Quuppa.Models;
using tUtils.Filters;

namespace tSync.Quuppa.Filters
{
    public class QuuppaTransformFilter : ChannelFilter<byte[], QuuppaData>
    {
        public QuuppaTransformFilter(ChannelReader<byte[]> channelReader, ChannelWriter<QuuppaData> channelWriter) : base(channelReader, channelWriter)
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
                var quuppaData = JsonSerializer.Deserialize<QuuppaData>(bytes, new JsonSerializerOptions() { 
                
                });
                Logger.LogTrace(quuppaData.ToString());
                await Writer.WriteAsync(quuppaData);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "");
            }
        }
    }
}
