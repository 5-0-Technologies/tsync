using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace tSync.ThingPark.Models
{
    public class Router
    {
        private readonly ILogger _logger;

        private readonly Dictionary<string, Action<HttpListenerContext>> GetRoutes = new();
        private readonly Dictionary<string, Action<HttpListenerContext>> PostRoutes = new();

        public Router(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public void Route(HttpListenerContext httpListenerContext)
        {
            try
            {
                string method = httpListenerContext.Request.HttpMethod.ToUpperInvariant();
                Uri uri = httpListenerContext.Request.Url;
                string path = uri.LocalPath;

                _logger.LogTrace("\tMethod: {0}\n\tUri: {1}", method, uri);
                Action<HttpListenerContext> action = null;
                switch (method)
                {
                    case "GET":
                        GetRoutes.TryGetValue(path, out action); break;
                    case "POST":
                        PostRoutes.TryGetValue(path, out action); break;
                    default: break;
                }

                if (action == null)
                {
                    _logger.LogTrace("Endpoint: {0} not found.", uri);
                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else
                {
                    action(httpListenerContext);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //httpListenerContext.Response.Close();
            }
        }

        public void Get(string pattern, Action<HttpListenerContext> callback)
        {
            GetRoutes.Add(pattern, callback);
        }

        public void Post(string pattern, Action<HttpListenerContext> callback)
        {
            PostRoutes.Add(pattern, callback);
        }
    }
}
