using System.Text.Json.Serialization;

namespace tSync.CommanderApi.Models
{
    public class CommanderVehicleResponse
    {
        [JsonPropertyName("vehicles")]
        public CommanderVehicle[] Vehicles { get; set; }
    }

    public class CommanderVehicle
    {
        [JsonPropertyName("vehicleId")]
        public int VehicleId { get; set; }

        [JsonPropertyName("vehicleName")]
        public string VehicleName { get; set; }

        [JsonPropertyName("vehicleRegistrationPlate")]
        public string VehicleRegistrationPlate { get; set; }

        [JsonPropertyName("vehicleDefaultDriver")]
        public int VehicleDefaultDriver { get; set; }

        [JsonPropertyName("lastCommunication")]
        public long LastCommunication { get; set; }

        [JsonPropertyName("deleted")]
        public int Deleted { get; set; }
    }
} 