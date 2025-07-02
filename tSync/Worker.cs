using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using tSync.CommanderApi;
using tSync.CommanderApi.Options;
using tSync.Options;
using tSync.Precog;
using tSync.Precog.Options;
using tSync.Quuppa;
using tSync.Quuppa.Options;
using tSync.RFControls;
using tSync.RFControls.Options;
using tSync.Simulator;
using tSync.Simulator.Options;
using tSync.Spin;
using tSync.Spin.Options;
using tSync.ThingPark;
using tSync.ThingPark.Options;
using tSync.Cisco;
using tSync.Cisco.Options;
using tUtils.Filters;

namespace tSync
{
    public class Worker : IHostedService
    {
        private readonly IOptions<tSyncOptions> configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly ICollection<Pipeline> pipelines;

        public Worker(IOptions<tSyncOptions> configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger(typeof(Worker));
            pipelines = new Collection<Pipeline>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var tSyncConfig = configuration.Value;
            
            QuuppaPipelineOptions[] quuppaOptions = tSyncConfig.Quuppa;
            if (quuppaOptions is not null)
            {
                foreach (var quuppaOption in quuppaOptions)
                {
                    try
                    {
                        quuppaOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        quuppaOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        quuppaOption.Channel ??= tSyncConfig.Channel ?? new();

                        QuuppaPipeline quuppaPipeline = new QuuppaPipeline(quuppaOption, loggerFactory);
                        pipelines.Add(quuppaPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            PrecogPipelineOptions[] precogOptions = tSyncConfig.Precog;
            if (precogOptions is not null)
            {
                foreach (var precogOption in precogOptions)
                {
                    try
                    {
                        precogOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        precogOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        precogOption.Channel ??= tSyncConfig.Channel ?? new();

                        var precogPipeline = new PrecogPipeline(precogOption, loggerFactory);
                        pipelines.Add(precogPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            SpinPipelineOptions[] spinOptions = tSyncConfig.Spin;
            if (spinOptions is not null)
            {
                foreach (var spinOption in spinOptions)
                {
                    try
                    {
                        spinOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        spinOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        spinOption.Channel ??= tSyncConfig.Channel ?? new();

                        var spinPipeline = new SpinPipeline(spinOption, loggerFactory);
                        pipelines.Add(spinPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            RFControlsPipelineOptions[] rfControlOptions = tSyncConfig.RFControls;
            if (rfControlOptions is not null)
            {
                foreach (var rfControlOption in rfControlOptions)
                {
                    try
                    {
                        rfControlOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        rfControlOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        rfControlOption.Channel ??= tSyncConfig.Channel ?? new();

                        var rFControlsPipeline = new RFControlsPipeline(rfControlOption, loggerFactory);
                        pipelines.Add(rFControlsPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            ThingParkPipelineOptions[] thingParkOptions = tSyncConfig.ThingPark;
            if (thingParkOptions is not null)
            {
                foreach (var thingParkOption in thingParkOptions)
                {
                    try
                    {
                        thingParkOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        thingParkOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        thingParkOption.Channel ??= tSyncConfig.Channel ?? new();

                        var thingParkPipeline = new ThingParkPipeline(thingParkOption, loggerFactory);
                        pipelines.Add(thingParkPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            SimulatorPipelineOptions[] twinzoSimulatorOptions = tSyncConfig.Simulator;
            if (twinzoSimulatorOptions is not null)
            {
                foreach (var twinzoSimulatorOption in twinzoSimulatorOptions)
                {
                    try
                    {
                        twinzoSimulatorOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        twinzoSimulatorOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        twinzoSimulatorOption.Channel ??= tSyncConfig.Channel ?? new();

                        var twinzoSimulatorPipeline = new SimulatorPipeline(twinzoSimulatorOption, loggerFactory);
                        pipelines.Add(twinzoSimulatorPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            CommanderApiPipelineOptions[] commanderApiOptions = tSyncConfig.CommanderApi;
            if (commanderApiOptions is not null)
            {
                foreach (var commanderApiOption in commanderApiOptions)
                {
                    try
                    {
                        commanderApiOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        commanderApiOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        commanderApiOption.Channel ??= tSyncConfig.Channel ?? new();

                        var commanderApiPipeline = new CommanderApiPipeline(commanderApiOption, loggerFactory);
                        pipelines.Add(commanderApiPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            CiscoPipelineOptions[] ciscoOptions = tSyncConfig.Cisco;
            if (ciscoOptions is not null)
            {
                foreach (var ciscoOption in ciscoOptions)
                {
                    try
                    {
                        ciscoOption.RtlsSender ??= tSyncConfig.RtlsSender ?? new();
                        ciscoOption.MemoryCache ??= tSyncConfig.MemoryCache ?? new();
                        ciscoOption.Channel ??= tSyncConfig.Channel ?? new();

                        var ciscoPipeline = new CiscoPipeline(ciscoOption, loggerFactory);
                        pipelines.Add(ciscoPipeline);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                    }
                }
            }

            foreach (var pipeline in pipelines)
            {
                pipeline.Start();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var pipeline in pipelines)
            {
                pipeline.Stop();
            }
            return Task.CompletedTask;
        }
    }
}
