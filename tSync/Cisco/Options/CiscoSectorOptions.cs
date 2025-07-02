using tSync.Model;

namespace tSync.Cisco.Options
{
    public class CiscoSectorOptions : IProviderSector
    {
        public string SectorId { get; set; }
        public float? OffsetX { get; set; }
        public float? OffsetY { get; set; }
    }
} 