using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using tSync.Options;

namespace tSync
{
    class Program
    {
        public static void Main(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
#if DEBUG
                    config.AddJsonFile("appSettings.Development.json", optional: false, reloadOnChange: true);
#else
                    config.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
#endif
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(options =>
                    {
                        options.AddSimpleConsole(c =>
                        {
                            c.TimestampFormat = "[HH:mm:ss] ";
                            c.ColorBehavior = LoggerColorBehavior.Enabled;
                            c.IncludeScopes = true;
                        });
                        options.SetMinimumLevel(LogLevel.Debug);
                    });
                    services.AddOptions<tSyncOptions>().Bind(hostContext.Configuration.GetSection(tSyncOptions.Name));
                    services.AddHostedService<Worker>();
                });

             hostBuilder.Build().Run();

            //try
            //{
            //    // Load tenant settings from config.json file.
            //    Data.LoadTenantsFromConfig();

            //    // Register  timers for each IDataSyncWorker
            //    ISyncWorker[] workers = new[] { new RtlsSyncWorker() };

            //    foreach (var worker in workers)
            //    {
            //        var timer = new System.Timers.Timer(worker.Interval);
            //        timer.AutoReset = true;
            //        timer.Enabled = true;
            //        timer.Elapsed += new ElapsedEventHandler(worker.SyncData);
            //        timer.Start();
            //    }

            //    // Workers are registered, run program forever and make keepalive every 10 minutes
            //    while (true)
            //        Thread.Sleep((int)TimeSpan.FromMinutes(10).TotalMilliseconds);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
        }
    }
}