using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using tSync.CommanderApi.Models;
using tSync.TwinzoApi;
using tUtils.Filters;

namespace tSync.CommanderApi.Filters
{
    public class CommanderVehicleNameFilter : Filter
    {
        private readonly string apiBaseUrl;
        private readonly string vehiclesEndpoint = "/vehicles";
        private readonly System.Timers.Timer timer;
        private readonly HttpClient httpClient;
        private readonly DevkitCacheConnector connector;
        private readonly Dictionary<int, CommanderVehicle> vehicleCache = new Dictionary<int, CommanderVehicle>();

        public CommanderVehicleNameFilter(
            string apiBaseUrl,
            string username,
            string password,
            DevkitCacheConnector connector,
            double updateIntervalMillis)
        {
            this.apiBaseUrl = apiBaseUrl ?? throw new ArgumentNullException(nameof(apiBaseUrl));
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
            
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add Basic Authentication header
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            timer = new System.Timers.Timer(updateIntervalMillis);
            timer.Elapsed += OnTimedEvent;
        }

        private async void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await FetchVehiclesAndUpdateDevices();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating vehicle names");
            }
        }

        private async Task FetchVehiclesAndUpdateDevices()
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}{vehiclesEndpoint}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<CommanderVehicleResponse>(content);

                if (apiResponse?.Vehicles != null)
                {
                    Logger.LogInformation($"Fetched {apiResponse.Vehicles.Length} vehicles from Commander API");
                    
                    // Update vehicle cache
                    var updatedVehicles = new Dictionary<int, CommanderVehicle>();
                    foreach (var vehicle in apiResponse.Vehicles)
                    {
                        if (vehicle.Deleted == 0) // Only process non-deleted vehicles
                        {
                            updatedVehicles[vehicle.VehicleId] = vehicle;
                        }
                    }

                    // Update device titles for all vehicles
                    foreach (var vehicle in updatedVehicles.Values)
                    {
                        await UpdateDeviceTitle(vehicle);
                    }

                    // Replace the cache with updated data
                    vehicleCache.Clear();
                    foreach (var kvp in updatedVehicles)
                    {
                        vehicleCache[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching vehicles from Commander API");
            }
        }

        private async Task UpdateDeviceTitle(CommanderVehicle vehicle)
        {
            try
            {
                var deviceId = vehicle.VehicleId.ToString();
                var device = await connector.GetDeviceByLogin(deviceId);
                
                if (device != null)
                {
                    if (device.Title != vehicle.VehicleName)
                    {
                        device.Title = vehicle.VehicleName;
                        await connector.UpdateDevice(device);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating device title for vehicle {vehicle.VehicleId}");
            }
        }

        protected override void BeforeRun()
        {
            // Fetch vehicles immediately on startup
            _ = FetchVehiclesAndUpdateDevices();
            timer.Start();
        }

        protected override void AfterRun()
        {
            timer.Stop();
            httpClient.Dispose();
        }

        public override Task Loop()
        {
            // The timer handles the polling, so we just need to keep the filter running
            return Task.Delay(-1);
        }
    }
} 