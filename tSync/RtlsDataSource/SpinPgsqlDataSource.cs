using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using tSync.Model;

namespace tSync.RtlsDataSource
{
    class SpinPgsqlDataSource : IRtlsDataSource
    {
        private string connectionString = "server=rtlsubuntu2.westeurope.cloudapp.azure.com;port=5432;user id=spinpgadmin;password=hA91fAg1_g2Bx6S;database={0};commandtimeout=0;keepalive=350;";
        private string twinzoBranchGuid;
        public double Interval => TimeSpan.FromSeconds(3).TotalMilliseconds;

        public IEnumerable<LocalizationRecord> GetLocalization()
        {
            return LoadData(SqlQueries.GetLocalization).AsEnumerable()
                .Select(r => new SpinLocalizationRecordFactory(r, twinzoBranchGuid));
        }

        /// <summary>
        /// Selects data from postgresql table
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private DataTable LoadData(string sql)
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using DataSet dataset = new DataSet();
            using var da = new NpgsqlDataAdapter(sql, connection);
            da.Fill(dataset);

            return dataset.Tables[0];
        }

        /// <summary>
        /// Build Spin PGSQL connection string by tenant
        /// </summary>
        /// <param name="sourceKey">PGSQL DB name</param>
        /// <returns></returns>
        public IRtlsDataSource? Build(params object[] data)
        {
            connectionString = string.Format(connectionString, data[0] as string);
            twinzoBranchGuid = data[1] as string;
            return this;
        }
    }
}
