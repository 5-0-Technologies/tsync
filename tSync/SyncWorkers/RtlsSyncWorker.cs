using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using tSync.Model;

namespace tSync.SyncWorkers
{
    class RtlsSyncWorker : ISyncWorker
    {
        public double Interval => TimeSpan.FromSeconds(3).TotalMilliseconds;

        public void SyncData(object sender, ElapsedEventArgs e)
        {
            SyncData();
        }
        public void SyncData()
        {
            Parallel.ForEach(Data.Tenants.Where(t => t.RtlsDataSource != null), async tenant =>
             {
                 try
                 {
                     // Define connection options with specific credentials and client identifiers
                     var api = await new TwinzoApi.TwinzoApi().BuildConnector(tenant).StartRtlsStateSynchronization(tenant.TwinzoBranchGuid);

                     // Load data from 3rdParty source ~ SPIN
                     var dataSource = tenant.RtlsDataSource.Build(tenant.RtlsConnectionString, tenant.TwinzoBranchGuid);
                     var data = dataSource.GetLocalization().ToList();

                     // Iterate devices and split registered and unregistered
                     var devicesToUpdate = new List<LocalizationRecord>();
                     foreach (var device in data)
                     {
                         if (TwinzoApi.TwinzoApi.registeredDevices[tenant.TwinzoBranchGuid].Any(rd => rd.Login == device.UserName))
                         {
                             // Device is registered, add id to collection to post
                             devicesToUpdate.Add(device);
                         }
                         else
                         {
                             // register new device
                             var newDevice = new DeviceContract()
                             {
                                 Title = device.UserName,
                                 Login = device.UserName,
                                 SectorId = (int)device.SectorId,
                                 DeviceTypeId = 4
                             };

                             await api.devkitConnector.AddDevice((DeviceWriteContract)newDevice);
                             TwinzoApi.TwinzoApi.registeredDevices[tenant.TwinzoBranchGuid].Add(newDevice);
                         }
                     }

                     // Convert Spin data to Twinzo data
                     var locationRecords = devicesToUpdate.Select(s => new DeviceLocationContract()
                     {
                         Login = s.UserName,
                         Locations = new LocationContract[]
                          {
                            new LocationContract()
                            {
                                IsMoving = s.IsMoving,
                                Battery = Convert.ToByte(s.Battery * 100),
                                X = s.X,
                                Y = s.Y,
                                SectorId = s.SectorId,
                                Timestamp = s.TimeStampMobile,
                                Interval = (int)tenant.RtlsDataSource.Interval
                            }
                          }
                     });

                     if (locationRecords.Any())
                     {
                         await api.devkitConnector.AddLocalizationData(locationRecords.ToArray());
                     }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex);
                 }

                 Console.WriteLine($"Tenant localization running: {tenant.TwinzoClientName}");
             });
        }
    }
}
