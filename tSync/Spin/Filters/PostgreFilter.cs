using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Data;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using tUtils;
using tUtils.Filters.Input;

namespace tSync.Spin.Filters
{
    public class PostgreFilter : InputChannelFilter<DataRow>
    {
        private readonly string connectionString;
        private readonly string selectCommand;
        private NpgsqlConnection connection;
        private long lastTS = DateTime.UtcNow.ToUnixTimestamp();

        public PostgreFilter(ChannelWriter<DataRow> channelWriter, string connectionString, string selectCommand) : base(channelWriter)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.selectCommand = selectCommand ?? throw new ArgumentNullException(nameof(selectCommand));
        }

        public override async Task Loop()
        {
            try
            {
                Logger.LogTrace($"{GetType().Name}: Executing sql ...");
                using (NpgsqlCommand command = new NpgsqlCommand(selectCommand, connection))
                {
                    command.Parameters.Add(new NpgsqlParameter("ts", lastTS / 1000));
                    Logger.LogTrace($"{GetType().Name}: {lastTS / 1000}");
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command))
                    {

                        DataSet dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        Logger.LogTrace($"{GetType().Name}: Sql executed ...");

                        if (dataset.Tables.Count > 0)
                        {
                            var rows = dataset.Tables[0].AsEnumerable();
                            Logger.LogTrace($"{GetType().Name}: Number of rows: {rows.Count()}");
                            foreach (var row in rows)
                            {
                                await Writer.WriteAsync(row, cancellationTokenSource.Token);
                            }
                        }
                        else
                        {
                            Logger.LogTrace($"{GetType().Name}: No Data.");
                        }
                    }
                }

                lastTS = DateTime.UtcNow.ToUnixTimestamp();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        protected override void AfterRun()
        {
            if (connection is not null)
            {
                connection.Close();
            }
        }

        protected override void BeforeRun()
        {
            try
            {
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
