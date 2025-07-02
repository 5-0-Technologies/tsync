using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Cisco.Options;
using tUtils.Filters.Input;

namespace tSync.Cisco.Filters
{
    public class HttpStreamListenerFilter : InputChannelFilter<byte[]>
    {
        private readonly HttpStreamOptions _options;
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public HttpStreamListenerFilter(ChannelWriter<byte[]> channelWriter, HttpStreamOptions options) : base(channelWriter)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();

            // Set timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            // Add headers
            if (_options.Headers != null)
            {
                foreach (var header in _options.Headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public override async Task Loop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    Logger.LogInformation($"{GetType().Name}: Connecting to Cisco Firehose stream at {_options.Url}");
                    
                    using var response = await _httpClient.GetAsync(_options.Url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream, Encoding.UTF8);

                    Logger.LogInformation($"{GetType().Name}: Connected to Cisco Firehose stream successfully");

                    string line;
                    while ((line = await reader.ReadLineAsync()) != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var bytes = Encoding.UTF8.GetBytes(line);
                            await Writer.WriteAsync(bytes, _cancellationTokenSource.Token);
                            Logger.LogTrace($"{GetType().Name}: Received data: {line}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInformation($"{GetType().Name}: Stream connection cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{GetType().Name}: Error in HTTP stream connection");
                    
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Logger.LogInformation($"{GetType().Name}: Retrying in {_options.RetryIntervalSeconds} seconds...");
                        await Task.Delay(TimeSpan.FromSeconds(_options.RetryIntervalSeconds), _cancellationTokenSource.Token);
                    }
                }
            }
        }

        protected override void BeforeRun()
        {
            Logger.LogInformation($"{GetType().Name}: Starting HTTP stream listener");
        }

        protected override void AfterRun()
        {
            _cancellationTokenSource.Cancel();
            _httpClient.Dispose();
            _cancellationTokenSource.Dispose();
            Logger.LogInformation($"{GetType().Name}: HTTP stream listener stopped");
        }
    }
} 