using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.CommanderApi.Models;
using tUtils.Filters;
using tUtils.Filters.Output;

namespace tSync.CommanderApi.Filters
{
    public class CommanderApiPollingFilter : Filter
    {
        private readonly string apiBaseUrl;
        private readonly string positionsEndpoint = "/last-positions";
        private readonly System.Timers.Timer timer;
        private readonly HttpClient httpClient;
        private readonly ChannelWriter<CommanderPosition> writer;

        public CommanderApiPollingFilter(
            ChannelWriter<CommanderPosition> channelWriter,
            string apiBaseUrl,
            string username,
            string password,
            double pollIntervalMillis)
        {
            this.writer = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
            this.apiBaseUrl = apiBaseUrl ?? throw new ArgumentNullException(nameof(apiBaseUrl));
            
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add Basic Authentication header
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            timer = new System.Timers.Timer(pollIntervalMillis);
            timer.Elapsed += OnTimedEvent;
        }

        private async void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}{positionsEndpoint}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<CommanderApiResponse>(content);

                if (apiResponse?.Positions != null)
                {
                    foreach (var position in apiResponse.Positions)
                    {
                        await writer.WriteAsync(position);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error polling Commander API");
            }
        }

        protected override void BeforeRun()
        {
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