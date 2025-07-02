using System.Text.Json;

namespace tSync.Spin.Models
{
    public class SpinLocationData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public string SectorId { get; set; }
        public bool? IsMoving { get; set; }
        public bool? IsFall { get; set; }
        public bool? IsPlugged { get; set; }
        public float? BatteryLevel { get; set; }
        public int? TimestampMobile { get; set; }
        public string ClientDeviceId { get; set; }
        public bool? PalletPresence { get; set; }
        public long? IsRecent { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
