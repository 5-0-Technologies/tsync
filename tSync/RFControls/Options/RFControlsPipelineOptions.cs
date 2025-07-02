using System.Text.Json;
using tSync.Model;
using tSync.Options;

namespace tSync.RFControls.Options
{
    public class RFControlsPipelineOptions
    {
        public const string Name = Providers.RFControls;

        public string[] Regions { get; set; }
        public int ReceiveInterval { get; set; }
        public MqttOptions Mqtt { get; set; }
        public ChannelOptions Channel { get; set; }
        public MemoryCacheOptions MemoryCache { get; set; }
        public RtlsSenderOptions RtlsSender { get; set; }
        public DevkitOptions Twinzo { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
