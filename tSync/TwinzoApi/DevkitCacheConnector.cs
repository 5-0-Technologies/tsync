using Microsoft.Extensions.Caching.Memory;
using SDK;
using SDK.Contracts.Communication;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using tSync.Model;
using tSync.Options;

namespace tSync.TwinzoApi
{
    public class DevkitCacheConnector
    {
        public DevkitConnectorV3 Connector { get; private set; }
        private readonly IMemoryCache memoryCache;

        public int ExpirationInSeconds { get; set; } = 60 * 10;

        public DevkitCacheConnector(DevkitConnectorV3 Connector, IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.Connector = Connector ?? throw new ArgumentNullException(nameof(Connector));
        }

        private async Task<IDictionary<int, BranchContract>> GetBranchData()
        {
            IDictionary<int, BranchContract> branches = null;
            if (!memoryCache.TryGetValue(CacheKeys.Branch, out branches))
            {
                var dBranches = await Connector.GetBranches();
                branches = dBranches.ToDictionary(d => d.Id);
                memoryCache.Set(CacheKeys.Branch, branches, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return branches;
        }

        public async Task<IDictionary<int, BranchContract>> GetBranchesDictionary()
        {
            return await GetBranchData();
        }

        public async Task<ICollection<BranchContract>> GetBranchesList()
        {
            return (await GetBranchData()).Values;
        }

        public async Task<BranchContract> GetBranchById(int id)
        {
            BranchContract branch;
            var branches = await GetBranchesDictionary();
            branches.TryGetValue(id, out branch);
            return branch;
        }

        public async Task<BranchContract> GetBranchByGuid(Guid branchGuid)
        {
            var branches = await GetBranchData();
            var stringGuid = branchGuid.ToString().ToUpperInvariant();
            foreach (var pair in branches)
            {
                if (pair.Value.Guid.ToUpperInvariant() == stringGuid)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        private async Task<IDictionary<int, DeviceContract>> GetDeviceData()
        {
            IDictionary<int, DeviceContract> devices = null;
            if (!memoryCache.TryGetValue(CacheKeys.Device, out devices))
            {
                var dDevice = await Connector.GetDevices();
                devices = dDevice.ToDictionary(d => d.Id);
                memoryCache.Set(CacheKeys.Device, devices, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return devices;
        }

        public async Task<DeviceContract> GetDeviceById(int id)
        {
            if (!memoryCache.TryGetValue(CacheKeys.Device + CacheKeys.Id, out ConcurrentDictionary<int, DeviceContract> devices))
            {
                devices = new ConcurrentDictionary<int, DeviceContract>();
                memoryCache.Set(CacheKeys.Device + CacheKeys.Id, devices, TimeSpan.FromSeconds(ExpirationInSeconds));
            }
            if (!devices.TryGetValue(id, out var device))
            {
                device = await Connector.GetDevice(id);
                if (device is { }) devices.TryAdd(id, device);
            }

            return device;
        }

        public async Task<DeviceContract> GetDeviceByLogin(string login)
        {
            if (!memoryCache.TryGetValue(CacheKeys.Device + CacheKeys.Login, out ConcurrentDictionary<string, DeviceContract> devices))
            {
                devices = new ConcurrentDictionary<string, DeviceContract>();
                memoryCache.Set(CacheKeys.Device + CacheKeys.Login, devices, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            // If we have a cached device, return it even if the API is down
            if (devices.TryGetValue(login, out var cachedDevice))
            {
                return cachedDevice;
            }

            try
            {
                Connector.EnsureSuccessStatusCode = false;
                var device = await Connector.GetDevice(login, "?$expand=Account");
                Connector.EnsureSuccessStatusCode = true;
                
                if (device != null)
                {
                    devices.TryAdd(login, device);
                }
                return device;
            }
            catch (TaskCanceledException ex)
            {
                // Handle timeout specifically
                throw new TimeoutException($"Timeout while getting device {login}", ex);
            }
        }

        public async Task<bool> ExistDeviceByLogin(string login)
        {
            try
            {
                var device = await GetDeviceByLogin(login);
                return device != null;
            }
            catch (TimeoutException)
            {
                // If we timeout, assume device doesn't exist
                return false;
            }
        }

        public async Task<DeviceContract> CreateDevice(DeviceContract deviceContract)
        {
            var tDevice = await Connector.AddDevice(((DeviceWriteContract)deviceContract));
            return tDevice;
        }

        public async Task<DeviceContract> UpdateDevice(DeviceContract deviceContract)
        {
            await Connector.UpdateDevice(deviceContract.Id, new { deviceContract.Title });
            IDictionary<int, DeviceContract> devices = await GetDeviceData();
            if (!devices.ContainsKey(deviceContract.Id))
            {
                devices.Add(deviceContract.Id, deviceContract);
            }

            return deviceContract;
        }

        public async Task<ManDownResponseContract[]> ManDownBatch(ManDownContract[] manDownBatchContracts)
        {
            return await Connector.ManDownBatch(manDownBatchContracts);
        }

        private async Task<IDictionary<int, SectorContract>> GetSectorData()
        {
            IDictionary<int, SectorContract> sectors;
            if (!memoryCache.TryGetValue(CacheKeys.Sector, out sectors))
            {
                var dSectors = await Connector.GetSectors("?$expand=GpsItems");
                sectors = dSectors.ToDictionary(d => d.Id);
                memoryCache.Set(CacheKeys.Sector, sectors, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return sectors;
        }

        public async Task<IDictionary<int, SectorContract>> GetSectorsDictionary()
        {
            return await GetSectorData();
        }

        public async Task<ICollection<SectorContract>> GetSectorsList()
        {
            return (await GetSectorData()).Values;
        }

        public async Task<SectorContract> GetSectorById(int Id)
        {
            (await GetSectorsDictionary()).TryGetValue(Id, out var sector);
            return sector;
        }

        private async Task<ConcurrentDictionary<string, TwinzoSector>> GetSectorsMappingNew()
        {
            ConcurrentDictionary<string, TwinzoSector> sectorMapping;

            // Check if the mapping is already cached
            if (!memoryCache.TryGetValue(CacheKeys.Sector, out sectorMapping))
            {
                sectorMapping = new ConcurrentDictionary<string, TwinzoSector>();
                var sectors = await GetSectorsList();  

                foreach (var sector in sectors)
                {
                    try
                    {

                        // Deserialize the configuration to TwinzoSector
                        var twinzoSector = new TwinzoSector();
                        twinzoSector.Sector = sector;

                        // Add the TwinzoSector to the mapping using the SectorId
                        sectorMapping.TryAdd(sector.Id.ToString().ToUpperInvariant(), twinzoSector);
                    }
                    catch (Exception)
                    {
                        // Handle any errors silently, if needed log or manage exceptions here
                    }
                }

                // Cache the sector mapping with an expiration time
                memoryCache.Set(CacheKeys.Sector, sectorMapping, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return sectorMapping;
        }
        public async Task<TwinzoSector> GetSector(string id)
        {
            var sectorMapping = await GetSectorsMappingNew();
            if (!sectorMapping.TryGetValue(id.ToUpperInvariant(), out var twinzoSector))
            {
                return null;
            }

            return twinzoSector;
        }


        private async Task<IDictionary<string, ConcurrentDictionary<string, TwinzoSector>>> GetSectorsMapping()
        {
            ConcurrentDictionary<string, ConcurrentDictionary<string, TwinzoSector>> providerMapping;
            if (!memoryCache.TryGetValue(CacheKeys.ProviderSector, out providerMapping))
            {
                providerMapping = new ConcurrentDictionary<string, ConcurrentDictionary<string, TwinzoSector>>();
                var sectors = await GetSectorsList();
                foreach (var sector in sectors)
                {
                    try
                    {
                        if (sector.Configuration is null)
                        {
                            continue;
                        }

                        var twinzoSector = JsonSerializer.Deserialize<TwinzoSector>(sector.Configuration);
                        if (twinzoSector != null)
                        {
                            twinzoSector.Sector = sector;

                            var providers = Providers.GetProviders();
                            foreach (var provider in providers)
                            {
                                var cacheKey = $"{provider}{CacheKeys.Sector}";
                                if (!providerMapping.TryGetValue(cacheKey, out var sectorMapping))
                                {
                                    sectorMapping = new ConcurrentDictionary<string, TwinzoSector>();
                                }
                                providerMapping.TryAdd(cacheKey, sectorMapping);

                                var propertyInfo = twinzoSector.GetType().GetProperty(provider);
                                if (propertyInfo is null)
                                {
                                    continue;
                                }

                                var propertyValue = propertyInfo.GetValue(twinzoSector);
                                if (propertyValue is IProviderSector providerSector)
                                {
                                    sectorMapping.TryAdd(providerSector.SectorId.ToUpperInvariant(), twinzoSector);
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
                memoryCache.Set(CacheKeys.ProviderSector, providerMapping, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return providerMapping;
        }

        public async Task<TwinzoSector> GetProviderSector(string id, string provider)
        {
            var providerMapping = await GetSectorsMapping();
            if (!providerMapping.TryGetValue($"{provider}{CacheKeys.Sector}", out var sectorMapping))
            {
                return null;
            }

            if (!sectorMapping.TryGetValue(id.ToUpperInvariant(), out var twinzoSector))
            {
                return null;
            }

            return twinzoSector;
        }

        private async Task<IDictionary<int, BeaconContract>> GetBeaconData()
        {
            IDictionary<int, BeaconContract> beacons;
            if (!memoryCache.TryGetValue(CacheKeys.Beacon, out beacons))
            {
                var dBeacons = await Connector.GetBeacons();
                beacons = dBeacons.ToDictionary(d => d.Id);
                memoryCache.Set(CacheKeys.Beacon, beacons, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return beacons;
        }

        public async Task<IDictionary<int, BeaconContract>> GetBeaconsDictionary()
        {
            return await GetBeaconData();
        }

        public async Task<ICollection<BeaconContract>> GetBeaconsList()
        {
            return (await GetBeaconData()).Values;
        }

        public async Task<BeaconContract> GetBeaconsById(int id)
        {
            BeaconContract beacon;
            var beacons = await GetBeaconData();
            beacons.TryGetValue(id, out beacon);
            return beacon;
        }

        public async Task<BeaconContract> GetBeaconsByTitle(string title)
        {
            var beacons = await GetBeaconData();
            foreach (var pair in beacons)
            {
                if (pair.Value.Title == title)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        private async Task<IDictionary<int, AreaContract>> GetAreaData()
        {
            IDictionary<int, AreaContract> areas;
            if (!memoryCache.TryGetValue(CacheKeys.Area, out areas))
            {
                var dAreas = await Connector.GetAreas();
                areas = dAreas.ToDictionary(d => d.Id);
                memoryCache.Set(CacheKeys.Area, areas, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return areas;
        }

        public async Task<IDictionary<int, AreaContract>> GetAreasDictionary()
        {
            return await GetAreaData();
        }

        public async Task<ICollection<AreaContract>> GetAreasList()
        {
            return (await GetAreaData()).Values;
        }

        public async Task<LayerContract[]> GetLocalizationLayers(string deviceLogin)
        {
            string key = $"{CacheKeys.LocalizationLayer}-{deviceLogin}";
            LayerContract[] localizationLayers;
            if (!memoryCache.TryGetValue(key, out localizationLayers))
            {
                localizationLayers = await Connector.GetLocalizationLayers(deviceLogin);
                memoryCache.Set(key, localizationLayers, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return localizationLayers;
        }

        private async Task<IDictionary<int, PathContract>> GetPathData()
        {
            IDictionary<int, PathContract> paths;
            if (!memoryCache.TryGetValue(CacheKeys.Path, out paths))
            {
                var dPaths = await Connector.GetPaths("?$expand=PathPoints");
                paths = dPaths.ToDictionary(d => d.Id);
                memoryCache.Set(CacheKeys.Path, paths, TimeSpan.FromSeconds(ExpirationInSeconds));
            }

            return paths;
        }

        public async Task<IDictionary<int, PathContract>> GetPathsDictionary()
        {
            return await GetPathData();
        }

        public async Task<ICollection<PathContract>> GetPathsList()
        {
            return (await GetPathData()).Values;
        }

        public async Task<PathContract> GetPathById(int id)
        {
            PathContract path;
            var paths = await GetPathData();
            paths.TryGetValue(id, out path);
            return path;
        }

        private static class CacheKeys
        {
            public static string Id => "Id";
            public static string Login => "Login";
            public static string Branch => "Branch";
            public static string Device => "Device";
            public static string Beacon => "Beacon";
            public static string Sector => "Sector";
            public static string ProviderSector => "ProviderSector";
            public static string Area => "Area";
            public static string LocalizationLayer => "LocalizationLayer";
            public static string Path => "Path";
        }
    }
}
