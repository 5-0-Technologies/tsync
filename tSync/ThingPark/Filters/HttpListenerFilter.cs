using Microsoft.Extensions.Logging;
using SDK.Contracts.Communication;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.ThingPark.Models;
using tSync.TwinzoApi;
using tUtils.Filters.Input;


namespace tSync.ThingPark.Filters
{
    public class HttpListenerFilter : InputChannelFilter<ThingParkData>
    {
        private readonly HttpServer httpServer;
        private readonly DevkitCacheConnector devkitCacheConnector;

        public HttpListenerFilter(ChannelWriter<ThingParkData> channelWriter,
            HttpServer httpServer,
            DevkitCacheConnector devkitCacheConnector) : base(channelWriter)
        {
            if (devkitCacheConnector is null)
            {
                throw new ArgumentNullException(nameof(devkitCacheConnector));
            }

            this.httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
            this.devkitCacheConnector = devkitCacheConnector;

            InitServices();
        }

        private void InitServices()
        {
            httpServer.Post("/", async (ctx) =>
            {
                if (!ctx.Request.HasEntityBody)
                {
                    BadRequest(ctx);
                    return;
                }

                try
                {
                    // Deserialize incoming JSON to DeviceMessage
                    var messagee = await JsonSerializer.DeserializeAsync<ThingParkData>(ctx.Request.InputStream);
                    Logger.LogTrace(messagee.ToString());

                    // Send the deserialized message to the channel
                    await Writer.WriteAsync(messagee);

                    // Respond OK
                    Ok(ctx, null);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing device message.");
                    BadRequest(ctx);
                }
            });
        }

        public override async Task Loop()
        {
            await Task.Delay(-1, cancellationTokenSource.Token);
        }

        private void NotFound(HttpListenerContext httpListenerContext)
        {
            Logger.LogTrace("Status code: {0}", HttpStatusCode.NotFound);
            httpListenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpListenerContext.Response.StatusDescription = "Not Found";
            httpListenerContext.Response.Close();
        }

        private void BadRequest(HttpListenerContext httpListenerContext)
        {
            Logger.LogTrace("Status code: {0}", HttpStatusCode.BadRequest);
            httpListenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpListenerContext.Response.StatusDescription = "Bad Request";
            httpListenerContext.Response.Close();
        }

        private void Ok(HttpListenerContext httpListenerContext, object content)
        {
            Logger.LogTrace("Status code: {0}", HttpStatusCode.OK);
            var buffer = JsonSerializer.SerializeToUtf8Bytes(content);
            httpListenerContext.Response.ContentType = "application/json";
            httpListenerContext.Response.ContentLength64 = buffer.Length;
            httpListenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            httpListenerContext.Response.StatusDescription = "OK";
            httpListenerContext.Response.KeepAlive = false;
            httpListenerContext.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public override void Start()
        {
            base.Start();
            //httpServer.Start();
        }

        public override void Stop()
        {
            base.Stop();
            httpServer.Stop();
        }

        protected override void AfterRun()
        {
            base.AfterRun();
        }

        protected override void BeforeRun()
        {
            base.BeforeRun();
        }



    }

}