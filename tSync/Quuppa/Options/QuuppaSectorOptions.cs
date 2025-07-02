using tSync.Model;

namespace tSync.Quuppa.Options
{
    public class QuuppaSectorOptions : IProviderSector
    {
        public string SectorId { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float OffsetZ { get; set; }
    }
}
