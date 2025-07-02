using SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace tSync.TwinzoApi
{
    public partial class TwinzoApi
    {
        internal static Dictionary<string, bool> RtlsSync = new Dictionary<string, bool>();
        public static Dictionary<string, SectorContract[]> sectors { get; } = new Dictionary<string, SectorContract[]>();
        public static Dictionary<string, List<DeviceContract>> registeredDevices { get; } = new Dictionary<string, List<DeviceContract>>();
        public static Dictionary<string, HashSet<string>> availableDeviceKeys { get; } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Async background fetch of RTLS related data with twinzo server
        /// 10 minutes to load new state from server
        /// Each minute add newly occured RTLS devices
        /// </summary>
        /// <param name="branchGuid">Clients branch guid</param>
        /// <returns></returns>
        public async Task<TwinzoApi> StartRtlsStateSynchronization(string branchGuid)
        {
            if (!sectors.ContainsKey(branchGuid))
            {
                sectors[branchGuid] = await devkitConnector.GetSectors();
            }

            if (!registeredDevices.ContainsKey(branchGuid))
            {
                registeredDevices[branchGuid] = new List<DeviceContract>(await devkitConnector.GetDevices());
            }

            if (!availableDeviceKeys.ContainsKey(branchGuid))
            {
                availableDeviceKeys[branchGuid] = new HashSet<string>();
            }

            if (!RtlsSync.ContainsKey(branchGuid))
            {
                RtlsSync[branchGuid] = true;
                sectors[branchGuid] = await devkitConnector.GetSectors();
                registeredDevices[branchGuid] = new List<DeviceContract>(await devkitConnector.GetDevices());

                var stateTimer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
                stateTimer.AutoReset = true;
                stateTimer.Enabled = true;
                stateTimer.Elapsed += async (s, e) =>
                {
                    Console.WriteLine($"Twinzo state loaded");
                    sectors[branchGuid] = await devkitConnector.GetSectors();
                    registeredDevices[branchGuid] = new List<DeviceContract>(await devkitConnector.GetDevices());
                };
                stateTimer.Start();
            }

            return this;
        }
    }
}
