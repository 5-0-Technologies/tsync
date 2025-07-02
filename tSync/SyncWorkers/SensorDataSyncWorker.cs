using System;
using System.Timers;

namespace tSync.SyncWorkers
{
    class SensorDataSyncWorker : ISyncWorker
    {
        public double Interval => TimeSpan.FromMinutes(5).TotalMilliseconds;

        public void SyncData(object sender, ElapsedEventArgs e)
        {
            SyncData();
        }
        public void SyncData()
        {

        }
    }
}
