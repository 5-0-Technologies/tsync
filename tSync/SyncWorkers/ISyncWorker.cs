using System.Timers;

namespace tSync.SyncWorkers
{
    interface ISyncWorker
    {
        double Interval { get; }
        public void SyncData();
        public void SyncData(object sender, ElapsedEventArgs e);
    }
}
