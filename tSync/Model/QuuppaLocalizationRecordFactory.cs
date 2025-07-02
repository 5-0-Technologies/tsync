using System;
using System.Collections.Generic;
using System.Text.Json;

namespace tSync.Model
{
    /// <summary>
    /// Quuppa BLE middleware
    /// JSON UDP stream
    /// https://quuppa.com/product-documentation/manuals/q/QSP/topics/QSP_udp_logging_api_output_formats.html
    /// </summary>
    class QuuppaLocalizationRecordFactory : LocalizationRecord
    {
        //float OffsetX = 5000;
        //float OffsetY = -8000;
        public QuuppaLocalizationRecordFactory(string payload, string twinzoBranchGuid)
        {
            Log.Message($"payload:{payload}");
            var data = JsonSerializer.Deserialize<QuuppaPayload>(payload);
            var sector = TwinzoApi.TwinzoApi.sectors[twinzoBranchGuid][0];

            Dictionary<string, SectorConfiguration> configuration = null;
            SectorConfiguration clientConfig = null;
            try
            {
                configuration = JsonSerializer.Deserialize<Dictionary<string, SectorConfiguration>>(sector.Configuration);
                clientConfig = configuration["quuppa"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (clientConfig == null)
            {
                clientConfig = new SectorConfiguration();
            }

            UserName = data.tagId;
            TimeStampMobile = data.locationTS;
            SectorId = sector.Id;
            X = clientConfig.OffsetX + data.location[0] * 1000;
            Y = clientConfig.OffsetY + Convert.ToSingle(sector.SectorHeight) - data.location[1] * 1000;
            Battery = 1;
            IsMoving = true;
        }

        class QuuppaPayload
        {
            public string tagId { get; set; }
            public long locationTS { get; set; }
            public float?[] location { get; set; }
            public string locationCoordSysId { get; set; }
        }

        class QuuppaZone
        {
            public string id { get; set; }
            public string name { get; set; }
        }
    }
}
