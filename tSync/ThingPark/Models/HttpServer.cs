using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace tSync.ThingPark.Models
{
    public class HttpServer
    {
        private readonly ILogger _logger;
        private readonly HttpListener _listener;
        private readonly Router _router;
        private CancellationTokenSource _cancellationTokenSource;

        public HttpServer(string[] prefixes, ILogger logger)
        {
            if (prefixes == null) throw new ArgumentNullException(nameof(prefixes));
            _logger = logger ?? NullLogger.Instance;
            _router = new Router(_logger);

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
        }

        private async Task Loop()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                HttpListenerContext ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                Task.Run(() => _router.Route(ctx), _cancellationTokenSource.Token);
            }
        }

        public void Start()
        {
            _logger.LogInformation("{0}: Starting.", GetType().Name);
            _logger.LogInformation("Listening at: {0}", string.Join(", ", _listener.Prefixes));
            _listener.Start();
            _cancellationTokenSource = new();
            Task.Run(Loop, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _logger.LogInformation("{0}: stopping.", GetType().Name);
            _cancellationTokenSource.Cancel();
            _listener.Stop();
        }

        public void Get(string pattern, Action<HttpListenerContext> callback)
        {
            _logger.LogTrace("Get method: {0}", pattern);
            _router.Get(pattern, callback);
        }

        public void Post(string pattern, Action<HttpListenerContext> callback)
        {
            _logger.LogTrace("Post method: {0}", pattern);
            _router.Post(pattern, callback);
        }
    }

}
