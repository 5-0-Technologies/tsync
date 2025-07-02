using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Model;
using tSync.Options;
using tSync.Spin.Models;
using tSync.TwinzoApi;
using tUtils;
using tUtils.Filters;

namespace tSync.Spin.Filters
{
    public class SpinLocationTransformFilter : ChannelFilter<SpinLocationData, DeviceLocationContract>
    {
        private readonly DevkitCacheConnector connector;
        private readonly Guid branchGuid;
        private readonly int spinIntervalMillis;
        private BranchContract branch;

        public SpinLocationTransformFilter(ChannelReader<SpinLocationData> channelReader,
            ChannelWriter<DeviceLocationContract> channelWriter,
            DevkitCacheConnector connector,
            Guid branchGuid,
            int spinIntervalMillis) : base(channelReader, channelWriter)
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
            this.spinIntervalMillis = spinIntervalMillis;
        }

        public override async Task Loop()
        {
            try
            {
                var spinLocation = await Reader.ReadAsync();

                if (spinLocation is null)
                {
                    Logger.LogWarning($"{GetType().Name}: Empty record. Skipped!");
                    return;
                }

                string login = Helper.RemoveSpecCharacters(spinLocation.Username);
                if (string.IsNullOrEmpty(login))
                {
                    Logger.LogWarning($"{GetType().Name}: Device has no name. Skipped!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(spinLocation.SectorId))
                {
                    Logger.LogWarning($"{GetType().Name}: Device {spinLocation.Username} empty sector identifier. Skipped!");
                    return;
                }

                var twinzoSector = await connector.GetProviderSector(spinLocation.SectorId, Providers.Spin);

                if (twinzoSector is null)
                {
                    Logger.LogWarning($"{GetType().Name}: Spin sector {spinLocation.SectorId} is not map to any twinzo sector. Skipped!");
                    return;
                }

                var branch = await GetBranch();
                if (branch is null)
                {
                    Logger.LogWarning($"{GetType().Name}: Branch {branchGuid} does not exist. Skipped!");
                    return;
                }

                if (!await connector.ExistDeviceByLogin(login))
                {
                    Logger.LogWarning($"{GetType().Name}: Device {spinLocation.Username} does not exist. Creating...");
                    await CreateDevice(spinLocation, branch.Id, twinzoSector.Sector.Id);
                }

                Logger.LogTrace($"{GetType().Name}: Creating localization record...");
                var deviceLocation = Map(spinLocation, twinzoSector);

                await Writer.WriteAsync(deviceLocation);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        public async Task<BranchContract> GetBranch()
        {
            if (branch is null)
            {
                branch = await connector.GetBranchByGuid(branchGuid);
            }

            return branch;
        }

        protected DeviceLocationContract Map(SpinLocationData spinLocation, TwinzoSector twinzoSector)
        {
            var location = new DeviceLocationContract()
            {
                Login = Helper.RemoveSpecCharacters(spinLocation.Username),
                Locations =
                [
                    new LocationContract()
                    {
                        SectorId = twinzoSector?.Sector.Id,
                        X = spinLocation.X.HasValue ? twinzoSector.Sector.SectorWidth - spinLocation.X : null,
                        Y = spinLocation.Y.HasValue ? twinzoSector.Sector.SectorHeight - spinLocation.Y * (-1) : null,
                        Battery = spinLocation.BatteryLevel.HasValue ? Convert.ToByte(spinLocation.BatteryLevel.Value * 100) : null,
                        IsMoving = spinLocation.IsMoving.HasValue ? spinLocation.IsMoving.Value : false,
                        Timestamp = spinLocation.TimestampMobile.HasValue ? ((long)spinLocation.TimestampMobile.Value) * 1000 : DateTime.UtcNow.ToUnixTimestamp(),
                        Interval = spinIntervalMillis,
                    }
                ]
            };

            return location;
        }

        public async Task<DeviceContract> CreateDevice(SpinLocationData spinLocationData, int branchId, int sectorId)
        {
            return await connector.CreateDevice(new DeviceContract()
            {
                Login = Helper.RemoveSpecCharacters(spinLocationData.Username),
                BranchId = branchId,
                SectorId = sectorId,
                Title = spinLocationData.Username,
                Position = false,
                Battery = spinLocationData.BatteryLevel ?? spinLocationData.BatteryLevel * 100,
                DeviceTypeId = 1,
            });
        }
    }
}
