using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.ThingPark.Models;
using tUtils.Filters;


namespace tSync.ThingPark.Filters
{
    internal class ThingParkTransformFilter : ChannelFilter<ThingParkData, ThingParkData>
    {
        public ThingParkTransformFilter(ChannelReader<ThingParkData> channelReader, ChannelWriter<ThingParkData> channelWriter) : base(channelReader, channelWriter)
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
                // Read the ThingParkData object directly from the channel
                var thingParkData = await Reader.ReadAsync();

                // Log the received ThingParkData object
                Console.WriteLine("Received ThingParkData: ");
                Console.WriteLine(thingParkData.ToString());

                Logger.LogTrace(thingParkData.ToString());

                // Send the ThingParkData object to the next writer in the pipeline
                await Writer.WriteAsync(thingParkData);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "");
            }
        }
    }
}
