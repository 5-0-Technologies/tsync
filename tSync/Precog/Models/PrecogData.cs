using System;

namespace tSync.Precog.Models
{
    public class PrecogData
    {
        public string BeaconName { get; set; }

        public string FourWords { get; set; }

        public string Mac { get; set; }

        public byte FrameType { get; set; }

        public string FrameTypeCode { get; set; }

        public short RSSI { get; set; }

        public string SSID { get; set; }

        public string SSID_ASCII { get; set; }

        public string AP_Mac { get; set; }

        public string AP_FourWords { get; set; }

        public string AP_CountryId { get; set; }

        public int Channel { get; set; }

        public string Fingerprint { get; set; }

        public string Fingerprint_ASCII { get; set; }

        public int? DwellTime { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
