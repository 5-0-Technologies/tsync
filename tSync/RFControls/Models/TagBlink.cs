using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tSync.RFControls.Models
{
    public class TagBlink
    {
        [JsonPropertyName("regionId")]
        public Guid RegionId { get; set; }

        [JsonPropertyName("antennaIds")]
        public Guid[] AntennaIds { get; set; }

        [JsonPropertyName("antennaNames")]
        public string[] AntennaNames { get; set; }

        [JsonPropertyName("tagID")]
        public string TagId { get; set; }

        [JsonPropertyName("x")]
        public float? X { get; set; }

        [JsonPropertyName("y")]
        public float? Y { get; set; }

        [JsonPropertyName("z")]
        public float? Z { get; set; }

        [JsonPropertyName("locateTime")]
        public long? LocateTime { get; set; }

        [JsonPropertyName("speed")]
        public float? Speed { get; set; }

        [JsonPropertyName("rssi")]
        public float? RSSI { get; set; }

        [JsonPropertyName("zoneName")]
        public string ZoneName { get; set; }

        [JsonPropertyName("zoneUUID")]
        public string ZoneUUID { get; set; }

        [JsonPropertyName("locationMethod")]
        public string LocationMethod { get; set; }

        [JsonPropertyName("Polarity")]
        public string Polarity { get; set; }

        [JsonPropertyName("confidenceInterval")]
        public float? ConfidenceInterval { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
