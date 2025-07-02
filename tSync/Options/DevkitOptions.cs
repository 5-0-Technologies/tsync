using System;
using System.Text.Json;

namespace tSync.Options
{
    public class DevkitOptions
    {
        public string TwinzoBaseUrl { get; set; }
        public Guid ClientGuid { get; set; }
        public Guid BranchGuid { get; set; }

        public int SectorId { get; set; }
        public string ApiKey { get; set; }
        public int Timeout { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
