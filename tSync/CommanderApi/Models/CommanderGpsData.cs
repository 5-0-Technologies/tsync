using System.Text.Json.Serialization;

namespace tSync.CommanderApi.Models
{
    public class CommanderApiResponse
    {
        [JsonPropertyName("positions")]
        public CommanderPosition[] Positions { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }

    public class CommanderPosition
    {
        [JsonPropertyName("vehicleId")]
        public int VehicleId { get; set; }

        [JsonPropertyName("gpsTime")]
        public long GpsTime { get; set; }

        [JsonPropertyName("gpsLat")]
        public float GpsLat { get; set; }

        [JsonPropertyName("gpsLon")]
        public float GpsLon { get; set; }

        [JsonPropertyName("voltage")]
        public float Voltage { get; set; }

        [JsonPropertyName("gpsSpeed")]
        public float GpsSpeed { get; set; }

        [JsonPropertyName("canSpeed")]
        public float CanSpeed { get; set; }
    }
} 