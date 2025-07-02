using System;
using System.Data;

namespace tSync.Model
{
    class SpinLocalizationRecordFactory : LocalizationRecord
    {
        private readonly DataRow row;

        /// <summary>
        /// Returns new LocalizationRecord populated with columns
        /// </summary>
        /// <param name="_row">PostgreSQL dataSource</param>
        /// <param name="twinzoBranchGuid">twinzo Branch guid</param>
        public SpinLocalizationRecordFactory(DataRow _row, string twinzoBranchGuid)
        {
            var sector = TwinzoApi.TwinzoApi.sectors[twinzoBranchGuid][0];

            row = _row;
            UserName = row["username"] as string;
            TimeStampMobile = (long)(int)row["timestampmobile"] * 1000;
            SectorId = sector.Id;
            X = Convert.ToSingle(sector.SectorWidth) - Convert.ToSingle(row["posx"]);
            Y = Convert.ToSingle(sector.SectorHeight) - Convert.ToSingle(row["posy"]) * (-1);
            Battery = row["battery_level"] as decimal?;
            IsMoving = row["is_moving"].Equals(true);
        }
    }
}
