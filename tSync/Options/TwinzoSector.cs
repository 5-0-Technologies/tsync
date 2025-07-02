using SDK.Models;
using tSync.Quuppa.Options;
using tSync.RFControls.Options;
using tSync.Spin.Options;
using tSync.ThingPark.Options;
using tSync.Cisco.Options;

namespace tSync.Options
{
    public class TwinzoSector
    {
        public SectorContract Sector { get; set; }
        public QuuppaSectorOptions Quuppa { get; set; }
        public ThingParkSectorOptions ThingPark { get; set; }
        public SpinSectorOptions Spin { get; set; }
        public RFControlsSectorOptions RFControls { get; set; }
        public CiscoSectorOptions Cisco { get; set; }
    }
}
