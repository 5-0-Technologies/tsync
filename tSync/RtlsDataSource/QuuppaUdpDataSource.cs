using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tSync.Model;

namespace tSync.RtlsDataSource
{
    /// <summary>
    /// Quuppa BLE middleware
    /// JSON UDP stream in DefaultLocation mode
    /// https://quuppa.com/product-documentation/manuals/q/QSP/topics/QSP_udp_logging_api_output_formats.html
    /// </summary>
    class QuuppaUdpDataSource : IRtlsDataSource
    {
        private const int Port = 9050;
        private static readonly ConcurrentQueue<LocalizationRecord> LocationRecords = new ConcurrentQueue<LocalizationRecord>();
        internal static Dictionary<string, bool> SyncBranches = new Dictionary<string, bool>();
        public double Interval => TimeSpan.FromSeconds(1).TotalMilliseconds;

        /// <summary>
        /// Build UDP client listener
        /// </summary>
        /// <param name="tcpStreamIP">string ip or dns adress of UDP stream</param>
        /// <param name="twinzoBranchGuid">string twinzo branch guid</param>
        /// <returns></returns>
        public IRtlsDataSource Build(params object[] data)
        {
            var tcpStreamIP = data[0] as string;
            var twinzoBranchGuid = data[1] as string;

            if (!SyncBranches.ContainsKey(twinzoBranchGuid))
            {
                new Thread(() =>
                {
                    SyncBranches.Add(twinzoBranchGuid, true);

                    Log.Message($"Opening UDP port:{Port} listener.");
                    var udpClient = new UdpClient(Port);
                    udpClient.BeginReceive(new AsyncCallback(OnUdpData), new object[] { udpClient, twinzoBranchGuid });

                    Log.Message($"UDP port:{Port} successfully receiving Quuppa data.");
                }).Start();
            }
            return this;
        }

        static void OnUdpData(IAsyncResult result)
        {
            var udpClient = (result.AsyncState as object[])[0] as UdpClient;
            var twinzoBranchGuid = (result.AsyncState as object[])[1] as string;
            var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port);

            try
            {
                var receivedBytes = udpClient.EndReceive(result, ref RemoteIpEndPoint);
                var x = new QuuppaLocalizationRecordFactory(Encoding.ASCII.GetString(receivedBytes), twinzoBranchGuid);
                Task.Run(() => LocationRecords.Enqueue(new QuuppaLocalizationRecordFactory(Encoding.ASCII.GetString(receivedBytes), twinzoBranchGuid)));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            finally
            {
                udpClient.BeginReceive(new AsyncCallback(OnUdpData), new object[] { udpClient, twinzoBranchGuid });
            }
        }

        public IEnumerable<LocalizationRecord> GetLocalization()
        {
            //return LocationRecords;
            return LocationRecords.DequeueChunk(1000);
        }
    }
}