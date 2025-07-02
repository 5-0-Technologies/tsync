using tSync.Precog.Options;
using tSync.Quuppa.Options;
using tSync.ThingPark.Options;
using tSync.RFControls.Options;
using tSync.Spin.Options;
using tSync.Simulator.Options;
using tSync.CommanderApi.Options;
using tSync.Cisco.Options;
using System.Text.Json;

namespace tSync.Options
{
    public class tSyncOptions
    {
        public const string Name = "tSync";
        public QuuppaPipelineOptions[] Quuppa { get; set; }
        public ThingParkPipelineOptions[] ThingPark { get; set; }
        public PrecogPipelineOptions[] Precog { get; set; }
        public SpinPipelineOptions[] Spin { get; set; }
        public RFControlsPipelineOptions[] RFControls { get; set; }
        public SimulatorPipelineOptions[] Simulator { get; set; }
        public CommanderApiPipelineOptions[] CommanderApi { get; set; }
        public CiscoPipelineOptions[] Cisco { get; set; }
        public ChannelOptions Channel { get; set; }
        public RtlsSenderOptions RtlsSender { get; set; }
        public MemoryCacheOptions MemoryCache { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
