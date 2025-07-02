using System.Text.Json;
using tSync.Options;

namespace tSync.Spin.Options
{
    public class SpinPipelineOptions
    {
        public const string Name = "Spin";
        public string SpinConnectionString { get; set; }
        public int SpinScanIntervalMillis { get; set; }
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
