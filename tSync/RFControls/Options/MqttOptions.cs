using System;
using System.Text.Json;

namespace tSync.RFControls.Options
{
    public class MqttOptions
    {
        public TimeSpan WithAutoReconnectDelay { get; set; }
        public string TcpServer { get; set; }
        public int? Port{ get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
