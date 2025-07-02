using System.Text.Json;
using tSync.Options;

namespace tSync.Quuppa.Options
{
    public class QuuppaPipelineOptions
    {
        public const string Name = "Quuppa";

        public UdpOptions UdpOptions { get; set; }
        public MqttOptions MqttOptions { get; set; }
        public int QuuppaScanIntervalMillis { get; set; }
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
