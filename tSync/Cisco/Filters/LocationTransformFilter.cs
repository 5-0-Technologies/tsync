using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Cisco.Models;
using tSync.Cisco.Options;
using tSync.Model;
using tSync.Options;
using tSync.ThingPark.Models;
using tSync.TwinzoApi;
using tUtils;
using tUtils.Filters;

namespace tSync.Cisco.Filters
{
    public class LocationTransformFilter : ChannelFilter<CiscoData, CiscoLocationWrapper>
    {
        private readonly DevkitCacheConnector _cacheConnector;
        private readonly Guid _branchGuid;
        private BranchContract _branch;
        private GpsToSectorConverter _gpsConverter;

        public LocationTransformFilter(ChannelReader<CiscoData> channelReader, ChannelWriter<CiscoLocationWrapper> channelWriter, DevkitCacheConnector cacheConnector, Guid branchGuid) : base(channelReader, channelWriter)
        {
            _cacheConnector = cacheConnector ?? throw new ArgumentNullException(nameof(cacheConnector));
            _branchGuid = branchGuid;
        }

        public override async Task Loop()
        {
            try
            {
                var ciscoData = await Reader.ReadAsync();
                
                if (ciscoData.EventType != "IOT_TELEMETRY" || ciscoData.IotTelemetry?.DetectedPosition == null)
                {
                    Logger.LogTrace($"{GetType().Name}: Skipping non-IOT_TELEMETRY event or null detected position");
                    return;
                }

                var detectedPosition = ciscoData.IotTelemetry.DetectedPosition;
                var deviceInfo = ciscoData.IotTelemetry.DeviceInfo;
                var macAddress = deviceInfo?.DeviceMacAddress;

                if (string.IsNullOrEmpty(macAddress))
                {
                    Logger.LogWarning($"{GetType().Name}: Missing MAC address in device info");
                    return;
                }

                if (string.IsNullOrEmpty(detectedPosition.MapId))
                {
                    Logger.LogWarning($"{GetType().Name}: Missing mapId in detected position");
                    return;
                }

                var branch = await GetBranch();
                if (branch is null)
                {
                    Logger.LogWarning($"Branch {_branchGuid} does not exist. Skipped.");
                    return;
                }

                // Get sector configuration dynamically using mapId
                var twinzoSector = await _cacheConnector.GetProviderSector(detectedPosition.MapId, Providers.Cisco);
                if (twinzoSector is null)
                {
                    Logger.LogWarning($"No sector configuration found for mapId: {detectedPosition.MapId}. Skipped.");
                    return;
                }

                // Initialize GPS converter if not already done
                if (_gpsConverter == null && twinzoSector.Sector?.GpsItems != null && twinzoSector.Sector.GpsItems.Length >= 2)
                {
                    var topLeft = new GpsItem
                    {
                        X = twinzoSector.Sector.GpsItems[0].X,
                        Y = twinzoSector.Sector.GpsItems[0].Y,
                        Latitude = twinzoSector.Sector.GpsItems[0].Latitude,
                        Longitude = twinzoSector.Sector.GpsItems[0].Longitude
                    };

                    var bottomRight = new GpsItem
                    {
                        X = twinzoSector.Sector.GpsItems[1].X,
                        Y = twinzoSector.Sector.GpsItems[1].Y,
                        Latitude = twinzoSector.Sector.GpsItems[1].Latitude,
                        Longitude = twinzoSector.Sector.GpsItems[1].Longitude
                    };

                    _gpsConverter = new GpsToSectorConverter(topLeft, bottomRight);
                }

                // For Cisco, we'll use the MAC address as the device login
                if (!await _cacheConnector.ExistDeviceByLogin(macAddress))
                {
                    Logger.LogWarning($"Device {macAddress} does not exist. Creating...");
                    await CreateDevice(macAddress, branch.Id, twinzoSector?.Sector?.Id);
                }

                var deviceLocation = Map(ciscoData, twinzoSector);

                var wrapper = new CiscoLocationWrapper
                {
                    CiscoData = ciscoData,
                    DeviceLocationContract = deviceLocation
                };

                Logger.LogTrace($"{GetType().Name}: Transformed location for device {macAddress}: MapId={detectedPosition.MapId}, Lat={detectedPosition.Latitude}, Lon={detectedPosition.Longitude}");
                await Writer.WriteAsync(wrapper);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, $"{GetType().Name}: Error transforming location data");
            }
        }

        protected DeviceLocationContract Map(CiscoData ciscoData, TwinzoSector twinzoSector)
        {
            var detectedPosition = ciscoData.IotTelemetry.DetectedPosition;
            var deviceInfo = ciscoData.IotTelemetry.DeviceInfo;
            
            float x, y;
            
            // Use GPS coordinates to compute x,y if converter is available and GPS coordinates are valid
            if (_gpsConverter != null && detectedPosition.Latitude != 0 && detectedPosition.Longitude != 0)
            {
                var (computedX, computedY) = _gpsConverter.ConvertGpsToSector(detectedPosition.Latitude, detectedPosition.Longitude);
                x = computedX;
                y = computedY;
            }
            else
            {
                // Fallback to using the xPos and yPos from Cisco as they are already in the correct coordinate system
                x = (float)detectedPosition.XPos;
                y = (float)detectedPosition.YPos;
            }
            
            // Apply sector offsets if configured
            if (twinzoSector?.Cisco != null)
            {
                x += twinzoSector.Cisco.OffsetX ?? 0;
                y += twinzoSector.Cisco.OffsetY ?? 0;
            }
            
            var location = new DeviceLocationContract()
            {
                Login = deviceInfo.DeviceMacAddress,
                Locations = new[]
                {
                    new LocationContract
                    {
                        SectorId = twinzoSector?.Sector?.Id,
                        X = x,
                        Y = y,
                        Z = 0, // Cisco doesn't provide Z coordinate in this format
                        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ciscoData.RecordTimestamp).DateTime.ToUnixTimestamp(),
                        IsMoving = false // Cisco doesn't provide movement status in this format
                    }
                }
            };

            return location;
        }

        public async Task<BranchContract> GetBranch()
        {
            return _branch ?? (_branch = await _cacheConnector.GetBranchByGuid(_branchGuid));
        }

        public async Task<DeviceContract> CreateDevice(string title, int branchId, int? sectorId)
        {
            return await _cacheConnector.CreateDevice(new DeviceContract()
            {
                Login = title,
                BranchId = branchId,
                SectorId = sectorId,
                Title = title,
                Position = false,
                DeviceTypeId = 1,
            });
        }
    }
} 