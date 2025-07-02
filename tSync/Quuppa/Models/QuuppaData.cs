 using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tSync.Quuppa.Models
{
    public class QuuppaData
    {
        [JsonPropertyName("tagId")]
        public string TagId { get; set; }

        [JsonPropertyName("tagName")]
        public string TagName { get; set; }

        [JsonPropertyName("locationMovementStatus")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LocationMovementStatus? LocationMovementStatus { get; set; }

        [JsonPropertyName("locationTS")]
        public long? LocationTS { get; set; }

        [JsonPropertyName("location")]
        public float?[] Location { get; set; }

        [JsonPropertyName("locationCoordSysId")]
        public Guid? LocationCoordSysId { get; set; }

        [JsonPropertyName("button1State")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ButtonState? Button1State { get; set; }

        [JsonPropertyName("button1StateTS")]
        public long? Button1StateTS { get; set; }

        [JsonPropertyName("button1LastPressTS")]
        public long? Button1LastPressTS { get; set; }

        [JsonPropertyName("batteryAlarm")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BatteryState? BatteryAlarm { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    [Description("The status indicating current tag movement type.")]
    public enum LocationMovementStatus : byte
    {
        [Description("The tag is in a zone that is not allow ed to report its position.")]
        Hidden,
        [Description("The system sees the tag as being on the move. Tag state is triggered.")]
        Moving,
        [Description(" The system has determined the tag is not currently moving. For example, tag has entered the default state.")]
        Stationary,
        [Description("The tag exists in the system but movement status cannot be determined (e.g. no location yet)")]
        NoData
    }

    public enum ButtonState : byte
    {
        NotPushed,
        Pushed
    }

    public enum BatteryState : byte
    {
        [Description("Battery is ok.")]
        Ok,
        [Description("Battery is getting low (tag should be retired soon).")]
        Low
    }
}
