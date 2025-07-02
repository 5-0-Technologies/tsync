using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using tSync.Model;

namespace tSync.RtlsDataSource
{
    /// <summary>
    /// Siemens UWB RTLS middleware
    /// SLMP via Socket (TCP/IP)
    /// https://cache.industry.siemens.com/dl/files/071/109764071/att_995152/v1/APH_RTLS-Datenexportdienst_76.pdf
    /// </summary>
    class SimaticRtlsDataSource : IRtlsDataSource
    {
        private const int Port = 4000;
        private static readonly ConcurrentQueue<LocalizationRecord> LocationRecords = new ConcurrentQueue<LocalizationRecord>();
        internal static Dictionary<string, bool> SyncBranches = new Dictionary<string, bool>();
        public double Interval => TimeSpan.FromSeconds(1).TotalMilliseconds;

        /// <summary>
        /// Build TCP client listener
        /// </summary>
        /// <param name="tcpStreamIP">string ip or dns adress of TCP stream</param>
        /// <param name="twinzoBranchGuid">string twinzo branch guid</param>
        /// <returns></returns>
        public IRtlsDataSource Build(params object[] data)
        {
            var tcpStreamIP = data[0] as string;
            var twinzoBranchGuid = data[1] as string;

            if (!SyncBranches.ContainsKey(tcpStreamIP))
            {
                new Thread(() =>
                {
                    SyncBranches.Add(tcpStreamIP, true);
                    Console.WriteLine("Opening connection to server.");
                    TcpClient client = new TcpClient(tcpStreamIP, Port);

                    Console.WriteLine("Reading data stream");
                    NetworkStream stream = client.GetStream();
                    byte[] data = new byte[1024];
                    int bytes;

                    while (true)
                    {
                        try
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            if (bytes > 0)
                            {
                                string responseData = Encoding.ASCII.GetString(data, 0, bytes);
                                Log.Message(string.Format("Received:\n{0}\n----------", responseData));

                                try
                                {
                                    using (StringReader reader = new StringReader(responseData))
                                    {
                                        string line = string.Empty;
                                        do
                                        {
                                            line = reader.ReadLine();
                                            Log.Message(line);
                                            if (line != null)
                                            {
                                                try
                                                {
                                                    LocationRecords.Enqueue(new SimaticLocalizationRecordFactory(line, twinzoBranchGuid));
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.Exception(ex);
                                                }
                                            }
                                        } while (line != null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Exception(ex);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    stream.Close();
                    client.Close();
                }).Start();
            }
            return this;
        }

        public IEnumerable<LocalizationRecord> GetLocalization()
        {
            return LocationRecords.DequeueChunk(1000);
        }
    }
}