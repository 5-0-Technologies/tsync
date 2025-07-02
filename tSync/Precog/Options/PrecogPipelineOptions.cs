using System.Text.Json;
using tSync.Options;

namespace tSync.Precog.Options
{
    public class PrecogPipelineOptions
    {
        public const string Name = "Precog";

        public string ConsumerGroup { get; set; }
        public string EventHubConnectionString { get; set; }
        public string MsSqlConnectionString { get; set; }
        public int AggregationIntervalMillis { get; set; }
        public RtlsSenderOptions RtlsSender { get; set; }
        public ChannelOptions Channel { get; set; }
        public MemoryCacheOptions MemoryCache { get; set; }
        public DevkitOptions Twinzo { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
