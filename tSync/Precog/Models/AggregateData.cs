using System.Collections.Generic;

namespace tSync.Precog.Models
{
    public class AggregateData
    {
        public AggregateData()
        {
            Devices = new Dictionary<string, Device>();
        }

        public long Timestamp { get; set; }

        public IDictionary<string, Device> Devices { get; set; }
    }

    public class Device
    {
        public Device()
        {
            Beacons = new Dictionary<string, Beacon>();
        }

        public string Name { get; set; }

        public IDictionary<string, Beacon> Beacons { get; set; }
    }

    public class Beacon
    {
        public string Name { get; set; }

        public float RSSI { get; set; }
    }


}
