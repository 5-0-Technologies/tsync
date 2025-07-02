using System.Collections.Generic;
using System.Text.Json;

namespace tSync.Cisco.Options
{
    public class HttpStreamOptions
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string ConnectionType { get; set; } = "http-stream";
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryIntervalSeconds { get; set; } = 5;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
} 