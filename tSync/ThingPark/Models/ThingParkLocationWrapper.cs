using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tSync.ThingPark.Models;

namespace tSync.ThingPark.Models
{
    public class ThingParkLocationWrapper
    {
        public ThingParkData ThingParkData { get; set; }

        public DeviceLocationContract DeviceLocationContract { get; set; }
    }
}
