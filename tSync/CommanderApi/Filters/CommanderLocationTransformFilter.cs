using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.CommanderApi.Models;
using tSync.ThingPark.Models;
using tSync.TwinzoApi;
using tUtils;
using tUtils.Filters;

namespace tSync.CommanderApi.Filters
{
    public class CommanderLocationTransformFilter : Filter
    {
        private readonly DevkitCacheConnector connector;
        private readonly Guid branchGuid;
        private readonly int sectorId;
        private readonly int pollIntervalMillis;
        private readonly ChannelReader<CommanderPosition> reader;
        private readonly ChannelWriter<DeviceLocationContract> writer;
        private GpsToSectorConverter gpsConverter;

        public CommanderLocationTransformFilter(
            ChannelReader<CommanderPosition> channelReader,
            ChannelWriter<DeviceLocationContract> channelWriter,
            DevkitCacheConnector connector,
            Guid branchGuid,
            int sectorId,
            int pollIntervalMillis)
        {
            this.reader = channelReader ?? throw new ArgumentNullException(nameof(channelReader));
            this.writer = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
            this.branchGuid = branchGuid;
            this.sectorId = sectorId;
            this.pollIntervalMillis = pollIntervalMillis;
        }

        public override async Task Loop()
        {
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var position))
                {
                    await ProcessData(position);
                }
            }
        }

        private async Task ProcessData(CommanderPosition position)
        {
            try
            {
                var deviceId = position.VehicleId.ToString();
                var branch = await connector.GetBranchByGuid(branchGuid);
                if (branch is null)
                {
                    Logger.LogWarning("No branch configuration. Skipped.");
                    return;
                }

                var twinzoSector = await connector.GetSectorById(sectorId);
                if (twinzoSector is null || twinzoSector.GpsItems == null || twinzoSector.GpsItems.Length < 2)
                {
                    Logger.LogWarning("No sector configuration or insufficient GPS items. Skipped.");
                    return;
                }

                if (gpsConverter == null)
                {
                    // Initialize GPS converter with sector boundaries
                    var topLeft = new GpsItem
                    {
                        X = twinzoSector.GpsItems[0].X,
                        Y = twinzoSector.GpsItems[0].Y,
                        Latitude = twinzoSector.GpsItems[0].Latitude,
                        Longitude = twinzoSector.GpsItems[0].Longitude
                    };

                    var bottomRight = new GpsItem
                    {
                        X = twinzoSector.GpsItems[1].X,
                        Y = twinzoSector.GpsItems[1].Y,
                        Latitude = twinzoSector.GpsItems[1].Latitude,
                        Longitude = twinzoSector.GpsItems[1].Longitude
                    };

                    gpsConverter = new GpsToSectorConverter(topLeft, bottomRight);
                }

                if (!await connector.ExistDeviceByLogin(deviceId))
                {
                    Logger.LogWarning($"Device {deviceId} does not exist. Creating...");
                    await CreateDevice(deviceId, branch.Id);
                }

                var deviceLocation = Map(position, twinzoSector);
                await writer.WriteAsync(deviceLocation);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        private async Task<DeviceContract> CreateDevice(string deviceId, int branchId)
        {
            return await connector.CreateDevice(new DeviceContract()
            {
                Title = deviceId,
                Login = deviceId,
                Position = false,
                BranchId = branchId,
                DeviceTypeId = 1,
                SectorId = sectorId,
                X = 0,
                Y = 0
            });
        }

        private DeviceLocationContract Map(CommanderPosition position, SectorContract sector)
        {
            // Convert GPS coordinates to sector coordinates
            var (x, y) = gpsConverter.ConvertGpsToSector(position.GpsLat, position.GpsLon);

            return new DeviceLocationContract()
            {
                Login = position.VehicleId.ToString(),
                Locations = new[]
                {
                    new LocationContract
                    {
                        SectorId = sectorId,
                        X = x,
                        Y = y,
                        Battery = (byte)(position.Voltage), // Convert voltage to battery percentage (rough estimate)
                        Interval = pollIntervalMillis,
                        IsMoving = position.GpsSpeed > 0 || position.CanSpeed > 0,
                        Timestamp = DateTime.Now.ToUnixTimestamp() //position.GpsTime * 1000
                    }
                }
            };
        }
    }
} 