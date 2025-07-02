using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tSync.ThingPark.Models
{
    public class GpsItem
    {
        public int Id { get; set; }
        public int SectorId { get; set; }
       // public Sector Sector { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Created { get; set; }
        public long Updated { get; set; }
        public int BranchId { get; set; }
      //  public Branch Branch { get; set; }
    }
}
