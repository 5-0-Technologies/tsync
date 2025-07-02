using System.Text.Json;
using tSync.Options;

namespace tSync.Cisco.Options
{
    public class CiscoPipelineOptions
    {
        public const string Name = "Cisco";

        public HttpStreamOptions HttpStreamOptions { get; set; }
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