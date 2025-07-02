using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.TwinzoApi;
using tUtils.Filters;

namespace tSync.Simulator.Filters
{
    public class SimulatorLocationTransformFilter : ChannelFilter<DeviceLocationContract, DeviceLocationContract>
    {
        private readonly DevkitCacheConnector connector;
        private readonly Guid branchGuid;
        private BranchContract branch;
        private const int RETRY_DELAY_MS = 60000; // 1 minute between retries

        public SimulatorLocationTransformFilter(
            ChannelReader<DeviceLocationContract> channelReader, 
            ChannelWriter<DeviceLocationContract> channelWriter,
            DevkitCacheConnector connector, 
            Guid branchGuid) : base(channelReader, channelWriter)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
            this.branchGuid = branchGuid;
        }

        public override async Task Loop()
        {
            try
            {
                var locationContract = await Reader.ReadAsync();

                if (locationContract is null || locationContract.Locations is null || locationContract.Locations.Length == 0)
                {
                    Logger?.LogWarning($"{GetType().Name}: Empty record. Skipped.");
                    return;
                }

                var branch = await GetBranch();
                if (branch is null)
                {
                    Logger?.LogWarning($"Branch {branchGuid} does not exist. Skipped.");
                    return;
                }

                if (!await connector.ExistDeviceByLogin(locationContract.Login))
                {
                    Logger?.LogWarning($"Device {locationContract.Login} does not exist. Creating...");
                    await CreateDevice(locationContract.Login, branch.Id, locationContract.Locations[0].SectorId);
                }

                await Writer.WriteAsync(locationContract);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error in SimulatorLocationTransformFilter - will retry in 1 minute");
                await Task.Delay(RETRY_DELAY_MS);
            }
        }

        private async Task<BranchContract> GetBranch()
        {
            return branch ?? (branch = await connector.GetBranchByGuid(branchGuid));
        }

        private async Task<DeviceContract> CreateDevice(string login, int branchId, int? sectorId)
        {
            return await connector.CreateDevice(new DeviceContract()
            {
                Login = login,
                BranchId = branchId,
                SectorId = sectorId,
                Title = login,
                Position = false,
                DeviceTypeId = 1,
                X = 0,
                Y = 0
            });
        }

        protected override void AfterRun()
        {
        }

        protected override void BeforeRun()
        {
        }
    }
} 