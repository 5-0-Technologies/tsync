using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.TwinzoApi;
using tUtils.Filters;

namespace tSync.Filters
{
    public class AreaFilter : ChannelFilter<DeviceLocationContract, DeviceLocationContract>
    {
        private readonly DevkitCacheConnector connector;

        public AreaFilter(ChannelReader<DeviceLocationContract> channelReader,
            ChannelWriter<DeviceLocationContract> channelWriter,
            DevkitCacheConnector connector)
            : base(channelReader, channelWriter)
        {
            if (channelReader is null)
            {
                throw new ArgumentNullException(nameof(channelReader));
            }

            if (channelWriter is null)
            {
                throw new ArgumentNullException(nameof(channelWriter));
            }

            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public override async Task Loop()
        {
            try
            {
                var locations = await Reader.ReadAsync(cancellationTokenSource.Token);
                await ComputeAreas(locations);
                await Writer.WriteAsync(locations, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        protected async Task ComputeAreas(DeviceLocationContract deviceLocationContract)
        {
            Logger.LogTrace($"{GetType().Name}: Computing areas ...");

            if (deviceLocationContract == null)
            {
                return;
            }

            var layers = await connector.GetLocalizationLayers(deviceLocationContract.Login);
            if (layers == null)
            {
                return;
            }

            List<int> noGoAreas = new List<int>();
            List<int> localizationAreas = new List<int>();

            foreach (var location in deviceLocationContract.Locations)
            {
                if (!location.SectorId.HasValue || !location.X.HasValue || !location.Y.HasValue)
                {
                    continue;
                }

                foreach (var layer in layers)
                {
                    foreach (var area in layer.Areas)
                    {
                        if (location.SectorId != area.SectorId)
                        {
                            continue;
                        }

                        if (area.Coordinates is null || area.Coordinates.Length < 2)
                        {
                            continue;
                        }

                        if (ContainsPoint(area.Coordinates, location.X.Value, location.Y.Value))
                        {
                            if (layer.IsNoGo)
                            {
                                noGoAreas.Add(area.Id);
                            }
                            else
                            {
                                localizationAreas.Add(area.Id);
                            }
                        }
                    }
                }
                location.LocalizationAreas = localizationAreas.ToArray();
                location.NoGoAreas = noGoAreas.ToArray();
                noGoAreas.Clear();
                localizationAreas.Clear();
            }
        }

        public bool ContainsPoint(PointContract[] polyPoints, float x, float y)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (((pi.Y <= y && y < pj.Y) || (pj.Y <= y && y < pi.Y)) &&
                    (x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
