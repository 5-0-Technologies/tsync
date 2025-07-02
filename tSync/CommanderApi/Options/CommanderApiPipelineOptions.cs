using System.Text.Json;
using tSync.Options;

namespace tSync.CommanderApi.Options
{
    public class CommanderApiPipelineOptions
    {
        public const string Name = "CommanderApi";

        public string ApiBaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int PollIntervalMillis { get; set; }
        public int VehicleNameUpdateIntervalMillis { get; set; } = 600000; // Default to 10 minutes
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