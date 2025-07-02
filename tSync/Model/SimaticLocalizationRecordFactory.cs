using System;
using System.Collections.Generic;
using System.Text.Json;
using tUtils;

namespace tSync.Model
{
    class SimaticLocalizationRecordFactory : LocalizationRecord
    {
        /// <summary>
        ///  TODO parametrize by 3rd party sector options
        /// </summary>
        const float OffsetX = 152438;
        const float OffsetY = 394422;

        /// <summary>
        /// SLMP via Socket (TCP/IP)
        /// https://cache.industry.siemens.com/dl/files/071/109764071/att_995152/v1/APH_RTLS-Datenexportdienst_76.pdf
        /// </summary>
        /// <param name="tcp_message"></param>
        /// <param name="twinzoBranchGuid"></param>
        public SimaticLocalizationRecordFactory(string tcp_message, string twinzoBranchGuid)
        {
            var data = tcp_message.Split(',');
            var sector = TwinzoApi.TwinzoApi.sectors[twinzoBranchGuid][0];

            Dictionary<string, SectorConfiguration> configuration = null;
            SectorConfiguration clientConfig = null;
            try
            {
                configuration = JsonSerializer.Deserialize<Dictionary<string, SectorConfiguration>>(sector.Configuration);
                clientConfig = configuration["simatic"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (clientConfig == null)
            {
                clientConfig = new SectorConfiguration();
            }

            UserName = data[3];
            TimeStampMobile = DateTime.Now.ToUnixTimestamp();
            // Parsing date from SIMATIC message removed, due to invalid format in period 00:00 - 00:59
            // DateTime.ParseExact(data[8], "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture).ToUnixTimestamp();
            SectorId = sector.Id;
            X = clientConfig.OffsetX + +Convert.ToSingle(data[4]) * 1000;
            Y = clientConfig.OffsetY - Convert.ToSingle(data[5]) * 1000;
            Battery = 1;
            IsMoving = true;
        }
    }
}
