using System.Text.Json;
using System.Text.Json.Serialization;

namespace tSync.Cisco.Models
{
    public class CiscoData
    {
        [JsonPropertyName("recordUid")]
        public string RecordUid { get; set; }

        [JsonPropertyName("recordTimestamp")]
        public long RecordTimestamp { get; set; }

        [JsonPropertyName("spacesTenantId")]
        public string SpacesTenantId { get; set; }

        [JsonPropertyName("spacesTenantName")]
        public string SpacesTenantName { get; set; }

        [JsonPropertyName("partnerTenantId")]
        public string PartnerTenantId { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        [JsonPropertyName("iotTelemetry")]
        public CiscoIotTelemetry IotTelemetry { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class CiscoIotTelemetry
    {
        [JsonPropertyName("deviceInfo")]
        public CiscoDeviceInfo DeviceInfo { get; set; }

        [JsonPropertyName("detectedPosition")]
        public CiscoDetectedPosition DetectedPosition { get; set; }

        [JsonPropertyName("location")]
        public CiscoLocation Location { get; set; }

        [JsonPropertyName("deviceRtcTime")]
        public long DeviceRtcTime { get; set; }

        [JsonPropertyName("rawHeader")]
        public int RawHeader { get; set; }

        [JsonPropertyName("rawPayload")]
        public string RawPayload { get; set; }

        [JsonPropertyName("sequenceNum")]
        public int SequenceNum { get; set; }

        [JsonPropertyName("maxDetectedRssi")]
        public int MaxDetectedRssi { get; set; }
    }

    public class CiscoDeviceInfo
    {
        [JsonPropertyName("deviceType")]
        public string DeviceType { get; set; }

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("deviceMacAddress")]
        public string DeviceMacAddress { get; set; }

        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; }

        [JsonPropertyName("firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonPropertyName("rawDeviceId")]
        public string RawDeviceId { get; set; }

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonPropertyName("companyId")]
        public string CompanyId { get; set; }

        [JsonPropertyName("serviceUuid")]
        public string ServiceUuid { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("vendorId")]
        public string VendorId { get; set; }

        [JsonPropertyName("deviceModel")]
        public string DeviceModel { get; set; }
    }

    public class CiscoDetectedPosition
    {
        [JsonPropertyName("xPos")]
        public double XPos { get; set; }

        [JsonPropertyName("yPos")]
        public double YPos { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("confidenceFactor")]
        public double ConfidenceFactor { get; set; }

        [JsonPropertyName("mapId")]
        public string MapId { get; set; }

        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("lastLocatedTime")]
        public long LastLocatedTime { get; set; }
    }

    public class CiscoLocation
    {
        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("inferredLocationTypes")]
        public string[] InferredLocationTypes { get; set; }

        [JsonPropertyName("parent")]
        public CiscoLocation Parent { get; set; }

        [JsonPropertyName("sourceLocationId")]
        public string SourceLocationId { get; set; }

        [JsonPropertyName("floorNumber")]
        public int? FloorNumber { get; set; }

        [JsonPropertyName("apCount")]
        public int ApCount { get; set; }
    }
} 