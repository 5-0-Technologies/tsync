using System.Text.Json;
using tSync.Options;

namespace tSync.ThingPark.Options
{
    public class ThingParkPipelineOptions
    {
        public const string Name = "ThingPark";

        public int ThingParkTcpPort { get; set; }
       
        public string ThingParkTcpAddress { get; set; }
        public HttpServerOptions HttpServer { get; set; }

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
