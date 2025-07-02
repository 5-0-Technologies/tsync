using System.Text.Json;

namespace tSync.Options
{
    public class RtlsSenderOptions
    {
        public const string Name = "RtlsSender";
        public int SendIntervalMillis { get; set; } = 10000;
        public int MaxSize { get; set; } = 50;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
