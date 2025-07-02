using System.Text.Json;
using tSync.Options;

namespace tSync.Simulator.Options
{
    public class SimulatorPipelineOptions
    {
        public const string Name = "Simulator";

        public int UpdateInterval { get; set; }

        public SimulatorPathOptions[] Paths { get; set; }
        public RtlsSenderOptions RtlsSender { get; set; }
        public ChannelOptions Channel { get; set; }
        public MemoryCacheOptions MemoryCache { get; set; }
        public DevkitOptions Twinzo { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class SimulatorPathOptions
    {
        public int PathId { get; set; }
        public string[] Devices { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
} 