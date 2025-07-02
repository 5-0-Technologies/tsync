using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Precog.Models;
using tUtils.Filters;

namespace tSync.Precog.Filters
{
    public class TransformatorFilter : ChannelFilter<byte, PrecogBeacon>
    {
        private readonly List<byte> buffer = new List<byte>();

        public TransformatorFilter(ChannelReader<byte> channelReader, ChannelWriter<PrecogBeacon> channelWriter) : base(channelReader, channelWriter)
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
            await Reader.WaitToReadAsync();
            buffer.Add(await Reader.ReadAsync());

            if (buffer.Count > 2 && buffer[buffer.Count - 1] == 49 && buffer[buffer.Count - 2] == 49)
            {
                string jsonString = Encoding.ASCII.GetString(buffer.ToArray(), 0, buffer.Count - 2);
                PrecogBeacon precogBeacon = JsonSerializer.Deserialize<PrecogBeacon>(jsonString);
                await Writer.WriteAsync(precogBeacon);
                buffer.Clear();
            }

        }
    }
}
