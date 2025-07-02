using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tSync.Precog.Models
{
    public class PrecogBeacon
    {
        [JsonPropertyName("m")]
        public string Name { get; set; } // the beacon name

        [JsonPropertyName("idxcount")]
        public int RecordCount { get; set; } // count of devices in the JSON packet

        [JsonPropertyName("mt")]
        public byte MessageType { get; set; } // message type (currently always 0)

        [JsonPropertyName("sn")]
        public long SeqNumber { get; set; } // sequence number. Each packet transmitted, this incremented by 1 so missed packets can be identified

        [JsonPropertyName("cn")]
        public int ConnCount { get; set; } // how many times the device has re-connected  to Azure IoT Hub

        [JsonPropertyName("g")]
        public PrecogGps[] PrecogGps { get; set; } // our data of 0..n devices

        [JsonPropertyName("d")]
        public PrecogDevice[] PrecogDevices { get; set; } // our data of 0..n devices
    }

    public class PrecogGps
    {
        [JsonPropertyName("t")]
        [JsonConverter(typeof(DateConverter))]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("a")]
        public string Latitude { get; set; }

        [JsonPropertyName("o")]
        public string Longitude { get; set; }
    }

    public class PrecogDevice
    {
        [JsonPropertyName("c")]
        public string CountryCode { get; set; } // reported country code of access point

        [JsonPropertyName("m")]
        public string Mac { get; set; } // mac address of device

        [JsonPropertyName("mf")]
        public string MacFourWords { get; set; }

        [JsonPropertyName("fw")]
        public int FrameType { get; set; } // frametype (see below)

        public string fp { get; set; }

        [JsonPropertyName("ch")]
        public byte Channel { get; set; }  // recorded channel

        [JsonPropertyName("h")]
        public string SSID { get; set; } // SSID of access point or list of SSIDs from probe requests from this device

        [JsonPropertyName("r")]
        public short RSSI { get; set; } // RSSI – signal strength

        [JsonPropertyName("cm")]
        public string ConnMac { get; set; } // mac address of access point if this device is connected to an access point

        public string ApFw { get; set; }

        [JsonPropertyName("st")]
        [JsonConverter(typeof(DateConverter))]
        public DateTime StartTimestamp { get; set; } // start timestamp

        [JsonPropertyName("et")]
        [JsonConverter(typeof(DateConverter))]
        public DateTime EndTimestamp { get; set; } // end timestamp

        [JsonPropertyName("d")]
        public int DwellTimestamp { get; set; } // dwelltime
    }

    public enum FrameType : int
    {
        Missing = -1,
        AccessPoint = 1,
        Device = 2,
        ConnectedDevice = 3,
        Unknown = 4,
        Randomised = 5,
    }

    public class DateConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                DateTime.ParseExact(reader.GetString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateTime dateTimeValue, JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString("yyyy-MM-dd  HH:mm:ss", CultureInfo.InvariantCulture));
    }

    public class G
    {
        public string t { get; set; }
        public string a { get; set; }
        public string o { get; set; }
    }

    public class D
    {
        public string c { get; set; }
        public string m { get; set; }
        public string fw { get; set; }
        public int f { get; set; }
        public int ch { get; set; }
        public string h { get; set; }
        public string fp { get; set; }
        public int r { get; set; }
        public string cm { get; set; }
        public string apfw { get; set; }
        public string st { get; set; }
        public string et { get; set; }
        public int d { get; set; }
    }

    public class Root
    {
        public string m { get; set; }
        public string deviceid { get; set; }
        public int idxcount { get; set; }
        public int mt { get; set; }
        public int sn { get; set; }
        public int cn { get; set; }
        public List<G> g { get; set; }
        public List<D> d { get; set; }
    }
}
