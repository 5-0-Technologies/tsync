using Microsoft.Extensions.Logging;
using PrecogSync.Models;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Core.Localization;
using tSync.Precog.Models;
using tSync.TwinzoApi;
using tUtils.Filters.Input;

namespace tSync.Precog.Filters
{
    public class TrillaterationFilter : InputChannelFilter<DeviceLocationContract>
    {
        private readonly DevkitCacheConnector tDataSource;
        private readonly IDictionary<long, AggregateData> dictionary;
        private readonly int intervalMs;
        private readonly TrilaterationStrategy trilaterationStrategy;

        public TrillaterationFilter(IDictionary<long, AggregateData> dictionary,
            ChannelWriter<DeviceLocationContract> channelWriter,
            DevkitCacheConnector twinzoDataSource,
            int intervalMs,
            TrilaterationStrategy trilaterationStrategy) : base(channelWriter)
        {
            this.tDataSource = twinzoDataSource ?? throw new ArgumentNullException(nameof(twinzoDataSource));
            this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            this.intervalMs = intervalMs;
            this.trilaterationStrategy = trilaterationStrategy;
        }

        public override async Task Loop()
        {
            try
            {
                var unixTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                var round = CeilTimestamp(unixTimestamp, intervalMs);

                var intervals = dictionary.Keys.Where(k => k < round);
                foreach (var interval in intervals)
                {
                    if (dictionary.TryGetValue(interval, out var data))
                    {
                        foreach (var pair in data.Devices.ToList())
                        {
                            var device = pair.Value;
                            if (device is null)
                            {
                                continue;
                            }

                            List<Tuple<float, BeaconContract>> tBeacons = new List<Tuple<float, BeaconContract>>();
                            foreach (var pair2 in device.Beacons)
                            {
                                var beacon = pair2.Value;
                                var tBeacon = await tDataSource.GetBeaconsByTitle(beacon.Name);
                                if (tBeacon is null || !tBeacon.SectorId.HasValue ||
                                    !tBeacon.X.HasValue || !tBeacon.Y.HasValue)
                                {
                                    continue;
                                }

                                tBeacons.Add(new Tuple<float, BeaconContract>(beacon.RSSI, tBeacon));
                            }

                            if (tBeacons.Count == 0)
                            {
                                continue;
                            }

                            var tDevice = await tDataSource.GetDeviceByLogin(device.Name);
                            if (tDevice is null)
                            {
                                tDevice = await CreateDevice(device.Name, tBeacons[0].Item2.BranchId);
                            }

                            var points = tBeacons.Select(t => new PointDistance()
                            {
                                X = (float)t.Item2.X,
                                Y = (float)t.Item2.Y,
                                Distance = (float)calculateDistance((int)t.Item1, -75)
                            }).OrderBy(p => p.Distance).ToArray();
                            var point = trilaterationStrategy.Localize(points);

                            if (point is not null)
                            {
                                var location = CreateLocationContract(point, interval, intervalMs, tDevice, tBeacons[0].Item2);
                                await Writer.WriteAsync(location);
                            }
                        }
                    }
                    dictionary.Remove(interval);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        public async Task<DeviceContract> CreateDevice(string title, int branchId)
        {
            return await tDataSource.CreateDevice(new DeviceContract()
            {
                Title = title,
                Login = title,
                Position = false,
                BranchId = branchId,
                DeviceTypeId = 1,
            });
        }

        public DeviceLocationContract CreateLocationContract(Point point, long timestamp, int interval, DeviceContract deviceContract, BeaconContract beaconContract)
        {
            return new DeviceLocationContract()
            {

                Login = deviceContract.Login,
                Locations = new LocationContract[]
                    {
                        new LocationContract()
                        {
                            Interval = interval,
                            SectorId = beaconContract.SectorId.Value,
                            X = point.X,
                            Y = point.Y,
                            Timestamp = timestamp,
                        }
                    }
            };
        }

        public static double calculateDistance(int rssi, int txPower)
        {
            double ratio = (rssi * 1.0 / txPower);
            if (ratio < 1.0)
            {
                return Math.Pow(ratio, 10);
            }
            else
            {
                return ((0.89976) * Math.Pow(ratio, 7.7095) + 0.111);
            }
        }

        public Point Localize(PointDistance[] pointDistances)
        {
            if (pointDistances.Length == 1)
            {
                return new Point()
                {
                    X = pointDistances[0].X,
                    Y = pointDistances[0].Y
                };
            }
            else if (pointDistances.Length == 2)
            {
                return new Point()
                {
                    X = pointDistances[0].X,
                    Y = pointDistances[0].Y
                };
            }
            else if (pointDistances.Length > 2)
            {
                float x, y;
                float i1 = pointDistances[0].X;
                float i2 = pointDistances[1].X;
                float i3 = pointDistances[2].X;
                float j1 = pointDistances[0].Y;
                float j2 = pointDistances[1].Y;
                float j3 = pointDistances[2].Y;
                float d1 = Math.Abs(pointDistances[0].Distance) * 1000;
                float d2 = Math.Abs(pointDistances[1].Distance) * 1000;
                float d3 = Math.Abs(pointDistances[2].Distance) * 1000;

                x = (((2 * j3 - 2 * j2) * ((d1 * d1 - d2 * d2) + (i2 * i2 - i1 * i1) + (j2 * j2 - j1 * j1)) - (2 * j2 - 2 * j1) * ((d2 * d2 - d3 * d3) + (i3 * i3 - i2 * i2) + (j3 * j3 - j2 * j2))) / ((2 * i2 - 2 * i3) * (2 * j2 - 2 * j1) - (2 * i1 - 2 * i2) * (2 * j3 - 2 * j2)));
                y = ((d1 * d1 - d2 * d2) + (i2 * i2 - i1 * i1) + (j2 * j2 - j1 * j1) + x * (2 * i1 - 2 * i2)) / (2 * j2 - 2 * j1);

                return new Point()
                {
                    X = x,
                    Y = y
                };
            }
            return null;
        }

        public long CeilTimestamp(long timestamp, long intervalMs)
        {
            return (long)Math.Ceiling((double)(timestamp / intervalMs)) * intervalMs;
        }
    }
}
