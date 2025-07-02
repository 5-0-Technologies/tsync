using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace tSync.ThingPark.Models
{
    public class ThingParkData
    {
        [JsonPropertyName("deviceEUI")]
        public string DeviceEUI { get; set; }

        [JsonPropertyName("coordinates")]
        public float[] Coordinates { get; set; }
    }
}
