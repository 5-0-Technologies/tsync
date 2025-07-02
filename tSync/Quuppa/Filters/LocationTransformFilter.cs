using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Model;
using tSync.Options;
using tSync.Quuppa.Models;
using tSync.TwinzoApi;
using tUtils;
using tUtils.Filters;

namespace tSync.Quuppa.Filters
{
    public class LocationTransformFilter : ChannelFilter<QuuppaData, QuuppaLocationWrapper>
    {
        private readonly DevkitCacheConnector connector;
        private readonly Guid branchGuid;
        private readonly int quuppaIntervalMillis;

        private BranchContract branch;

        public LocationTransformFilter(ChannelReader<QuuppaData> channelReader, ChannelWriter<QuuppaLocationWrapper> channelWriter,
            DevkitCacheConnector connector, Guid branchGuid, int quuppaIntervalMillis) : base(channelReader, channelWriter)
        {
            if (channelReader is null)
            {
                throw new ArgumentNullException(nameof(channelReader));
            }

            if (channelWriter is null)
            {
                throw new ArgumentNullException(nameof(channelWriter));
            }

            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
            this.branchGuid = branchGuid;
            this.quuppaIntervalMillis = quuppaIntervalMillis;
        }

        public override async Task Loop()
        {
            try
            {
                var quuppaData = await Reader.ReadAsync();

                if (quuppaData is null)
                {
                    Logger.LogWarning($"{GetType().Name}: Empty record. Skipped.");
                    return;
                }

                if (!quuppaData.LocationCoordSysId.HasValue)
                {
                    Logger.LogWarning("No locationCoordSysId. Skipped.");
                    return;
                }

                var branch = await GetBranch();
                if (branch is null)
                {
                    Logger.LogWarning($"Branch {branchGuid} does not exist. Skipped.");
                    return;
                }

                var twinzoSector = await connector.GetProviderSector(quuppaData.LocationCoordSysId.ToString(), Providers.Quuppa);
                if (twinzoSector is null)
                {
                    Logger.LogWarning("No sector configuration. Skipped.");
                    return;
                }

                if (!await connector.ExistDeviceByLogin(quuppaData.TagId))
                {
                    Logger.LogWarning($"Device {quuppaData.TagId} does not exist. Creating...");
                    await CreateDevice(quuppaData.TagId, branch.Id, twinzoSector?.Sector?.Id);
                }

                var deviceLocation = Map(quuppaData, twinzoSector);

                await Writer.WriteAsync(new QuuppaLocationWrapper()
                {
                    QuuppaData = quuppaData,
                    DeviceLocationContract = deviceLocation
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        protected DeviceLocationContract Map(QuuppaData quuppaData, TwinzoSector twinzoSector)
        {
            float? x = quuppaData?.Location?.Length > 0 ? quuppaData.Location[0] : null;
            float? y = quuppaData?.Location?.Length > 1 ? quuppaData.Location[1] : null;
            float? z = quuppaData?.Location?.Length > 2 ? quuppaData.Location[2] : null;

            x = twinzoSector?.Quuppa?.OffsetX + x * 1000;
            y = twinzoSector?.Quuppa?.OffsetY + (twinzoSector?.Sector?.SectorHeight) - (y * 1000);
            z = twinzoSector?.Quuppa?.OffsetZ + z * 1000;

            var location = new DeviceLocationContract()
            {
                Login = quuppaData.TagId,

                Locations = new[]
                {
                    new LocationContract
                    {
                        SectorId = twinzoSector?.Sector?.Id,
                        X = x,
                        Y = y,
                        Z = z,
                        Battery = quuppaData.BatteryAlarm is { } ? quuppaData.BatteryAlarm.Value == BatteryState.Ok ? (byte?)100 : 0 : null,
                        Interval = quuppaIntervalMillis,
                        IsMoving = quuppaData.LocationMovementStatus is LocationMovementStatus.Moving,
                        Timestamp = quuppaData.LocationTS ?? DateTime.UtcNow.ToUnixTimestamp()
                    }
                }
            };

            return location;
        }

        public async Task<BranchContract> GetBranch()
        {
            return branch ?? (branch = await connector.GetBranchByGuid(branchGuid));
        }

        public async Task<DeviceContract> CreateDevice(string title, int branchId, int? sectorId)
        {
            return await connector.CreateDevice(new DeviceContract()
            {
                Login = title,
                BranchId = branchId,
                SectorId = sectorId,
                Title = title,
                Position = false,
                DeviceTypeId = 1,
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
