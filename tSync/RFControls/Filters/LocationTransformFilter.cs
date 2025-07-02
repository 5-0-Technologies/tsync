using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Model;
using tSync.RFControls.Models;
using tSync.TwinzoApi;
using tUtils.Filters;

namespace tSync.RFControls.Filters
{
    public class LocationTransformFilter : ChannelFilter<TagBlink, DeviceLocationContract>
    {
        private readonly DevkitCacheConnector connector;
        private readonly Guid branchGuid;
        private readonly int intervalMillis;

        private const int MinAggInterval = 100;
        private const byte Battery = 100;
        private const int MM = 1000;

        private BranchContract branch;

        public LocationTransformFilter(ChannelReader<TagBlink> channelReader, ChannelWriter<DeviceLocationContract> channelWriter,
            DevkitCacheConnector cacheConnector,
            Guid branchGuid, int intervalMillis) : base(channelReader, channelWriter)
        {
            if (channelReader == null)
            {
                throw new ArgumentNullException(nameof(channelReader));
            }

            if (channelWriter == null)
            {
                throw new ArgumentNullException(nameof(channelWriter));
            }

            this.connector = cacheConnector ?? throw new ArgumentNullException(nameof(cacheConnector));
            this.branchGuid = branchGuid;

            if (intervalMillis < MinAggInterval)
            {
                throw new ArgumentOutOfRangeException($"Aggregation interval must be greater than {MinAggInterval}ms");
            }

            this.intervalMillis = intervalMillis;
        }

        public override async Task Loop()
        {
            try
            {
                var tagBlink = await Reader.ReadAsync(cancellationTokenSource.Token);
                if (tagBlink == null)
                {
                    return;
                }

                if (tagBlink.RegionId == Guid.Empty)
                {
                    Logger.LogWarning("Empty RegionId. Skipped");
                    return;
                }

                if (tagBlink.LocateTime is null)
                {
                    Logger.LogWarning("Empty LocateTime. Skipped");
                    return;
                }

                if (string.IsNullOrWhiteSpace(tagBlink.TagId))
                {
                    Logger.LogWarning("Empty TagId. Skipped");
                    return;
                }

                if (string.IsNullOrWhiteSpace(tagBlink.ZoneUUID))
                {
                    Logger.LogWarning("Empty ZoneUUID. Skipped");
                    return;
                }

                var zoneUUIDs = tagBlink.ZoneUUID.Split(',');
                if (!Guid.TryParse(zoneUUIDs[0], out _))
                {
                    Logger.LogWarning("No zone. Skipped.");
                    return;
                }

                var twinzoSector = await connector.GetProviderSector(zoneUUIDs[0], Providers.RFControls);
                if (twinzoSector is null)
                {
                    Logger.LogWarning("No sector configuration. Skipped.");
                    return;
                }

                var branch = await GetBranch();
                if (branch is null)
                {
                    Logger.LogWarning($"Branch {branchGuid} does not exist. Skipped.");
                    return;
                }

                if (!await connector.ExistDeviceByLogin(tagBlink.TagId))
                {
                    Logger.LogWarning($"Device {tagBlink.TagId} does not exist. Creating...");
                    await CreateDevice(tagBlink.TagId, branch.Id);
                }

                var dlc = new DeviceLocationContract
                {
                    Login = tagBlink.TagId,
                    Locations = new[]
                    {
                        new LocationContract
                        {
                            Interval = intervalMillis,
                            SectorId = twinzoSector.Sector.Id,
                            X = tagBlink.X * MM,
                            Y = twinzoSector.Sector.SectorHeight - tagBlink.Y * MM,
                            Z = tagBlink.Z * MM,
                            Timestamp = tagBlink.LocateTime.Value,
                            IsMoving = tagBlink.Speed is > 0f,
                            Battery = Battery
                        }
                    }
                };

                await Writer.WriteAsync(dlc);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "");
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

        public async Task<DeviceContract> CreateDevice(string title, int branchId)
        {
            return await connector.CreateDevice(new DeviceContract()
            {
                Login = title,
                BranchId = branchId,
                Title = title,
                Position = false,
                DeviceTypeId = 1,
            });
        }
    }
}
