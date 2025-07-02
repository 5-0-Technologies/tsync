using tSync.Model;

namespace tSync.Spin.Options
{
    public class SpinSectorOptions : IProviderSector
    {
        public string SectorId { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
    }
}
