using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Options;
using tSync.ThingPark.Models;
using tSync.ThingPark.Options;
using tSync.TwinzoApi;
using tUtils;
using tUtils.Filters;

namespace tSync.ThingPark.Filters
{
    public class ThingParkLocationTransformFilter : ChannelFilter<ThingParkData, ThingParkLocationWrapper>
    {
        private readonly DevkitCacheConnector connector;
        private readonly Guid branchGuid;
        private readonly int sectorId;
        private readonly ThingParkPipelineOptions opt;

        private BranchContract branch;

        public ThingParkLocationTransformFilter(ChannelReader<ThingParkData> channelReader, ChannelWriter<ThingParkLocationWrapper> channelWriter,
            DevkitCacheConnector connector, Guid branchGuid, int sectorId) : base(channelReader, channelWriter)
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
            this.sectorId = sectorId;
        }

        public override async Task Loop()
        {
            try
            {
                var thingParkData = await Reader.ReadAsync();

                if (thingParkData is null)
                {
                    Logger.LogWarning($"{GetType().Name}: Empty record. Skipped.");
                    return;
                }

                var branch = await GetBranch();
                if (branch is null)
                {
                    Logger.LogWarning($"Branch {branchGuid} does not exist. Skipped.");
                    return;
                }

                var sectorID = sectorId.ToString();
                var twinzoSector = await connector.GetSector(sectorID);

                if (twinzoSector is null)
                {
                    Logger.LogWarning("No sector configuration. Skipped.");
                    return;
                }

                if (!await connector.ExistDeviceByLogin(thingParkData.DeviceEUI))
                {
                    Logger.LogWarning($"Device {thingParkData.DeviceEUI} does not exist. Creating...");
                    await CreateDevice(thingParkData.DeviceEUI, branch.Id, twinzoSector?.Sector?.Id);
                }

                var deviceLocation = Map(thingParkData, twinzoSector);

                await Writer.WriteAsync(new ThingParkLocationWrapper()
                {
                    ThingParkData = thingParkData,
                    DeviceLocationContract = deviceLocation
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }
        protected DeviceLocationContract Map(ThingParkData thingParkData, TwinzoSector twinzoSector)
        {
            float? latitude = thingParkData?.Coordinates?.Length > 0 ? thingParkData.Coordinates[0] : null;
            float? longitude = thingParkData?.Coordinates?.Length > 1 ? thingParkData.Coordinates[1] : null;
            float? altitude = thingParkData?.Coordinates?.Length > 2 ? thingParkData.Coordinates[2] : null;

            if (latitude == null || longitude == null)
            {
                throw new InvalidOperationException("Invalid GPS coordinates in ThingParkData");
            }

            //Gps Items need to be corners
            var gpsItems = twinzoSector.Sector.GpsItems;

            if (gpsItems == null || gpsItems.Length < 2)
            {
                throw new InvalidOperationException("Sector must have at least two GPS items defining its boundaries.");
            }

            // Assuming the first two GPS items define the top-left and bottom-right corners
            var topLeftGps = gpsItems[0];
            var bottomRightGps = gpsItems[1];

            var topLeft = new GpsItem
            {
                X = topLeftGps.X,
                Y = topLeftGps.Y,
                Latitude = topLeftGps.Latitude,
                Longitude = topLeftGps.Longitude
            };

            var bottomRight = new GpsItem
            {
                X = bottomRightGps.X,
                Y = bottomRightGps.Y,
                Latitude = bottomRightGps.Latitude,
                Longitude = bottomRightGps.Longitude
            };

            // Initialize the GpsToSectorConverter with the sector's boundaries
            var gpsConverter = new GpsToSectorConverter(topLeft, bottomRight);

            // Convert incoming GPS coordinates to sector X, Y
            var (x, y) = gpsConverter.ConvertGpsToSector(latitude.Value, longitude.Value);

            var location = new DeviceLocationContract()
            {
                Login = thingParkData.DeviceEUI,
                Locations = new[]
                {
                    new LocationContract
                    {
                        SectorId = twinzoSector?.Sector?.Id,
                        X = x,
                        Y = y,
                        IsMoving = true,
                        Timestamp = DateTime.UtcNow.ToUnixTimestamp()
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
