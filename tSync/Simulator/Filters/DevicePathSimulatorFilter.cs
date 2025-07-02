using Microsoft.Extensions.Logging;
using SDK.Contracts.Data;
using SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using tSync.Simulator.Options;
using tSync.TwinzoApi;
using tUtils.Filters;

namespace tSync.Simulator.Filters
{
    public class DevicePathSimulatorFilter : Filter
    {
        private readonly DevkitCacheConnector connector;
        private readonly SimulatorPathOptions[] pathOptions;
        private readonly Random random;
        private readonly Dictionary<string, DeviceState> deviceStates;
        private readonly ChannelWriter<DeviceLocationContract> writer;
        private const float MOVEMENT_STEP = 0.20f; // Progress increment per update (2% per step)
        private int UPDATE_INTERVAL_MS = 1000;

        public DevicePathSimulatorFilter(
            ChannelWriter<DeviceLocationContract> writer,
            DevkitCacheConnector connector,
            SimulatorPathOptions[] pathOptions,
            int updateIntervalMs)
        {
            this.writer = writer;
            this.connector = connector;
            this.pathOptions = pathOptions;
            this.UPDATE_INTERVAL_MS = updateIntervalMs;
            this.random = new Random();
            this.deviceStates = new Dictionary<string, DeviceState>();
        }

        private class DeviceState
        {
            public PathPointContract[] PathPoints { get; set; }
            public float CurrentX { get; set; }
            public float CurrentY { get; set; }
            public int SectorId { get; set; }
            public float Progress { get; set; }
            public int CurrentIndex { get; set; }
            public int NextIndex { get; set; }
            public DateTimeOffset LastUpdateTime { get; set; }
            public bool IsMovingForward { get; set; } = true;
        }

        public override async Task Loop()
        {
            try
            {
                // Initialize device states if not already done
                foreach (var pathOpt in pathOptions)
                {
                    foreach (var deviceLogin in pathOpt.Devices)
                    {
                        if (!deviceStates.ContainsKey(deviceLogin))
                        {
                            var path = await connector.GetPathById(pathOpt.PathId);
                            if (path?.PathPoints == null || path.PathPoints.Length < 2)
                            {
                                Logger?.LogWarning($"Path {pathOpt.PathId} not found or invalid for device {deviceLogin}");
                                continue;
                            }

                            var orderedPoints = path.PathPoints.OrderBy(p => p.Index).ToArray();
                            var randomStartIndex = random.Next(0, orderedPoints.Length);
                            var nextIndex = randomStartIndex == orderedPoints.Length - 1 ? 
                                randomStartIndex - 1 : 
                                randomStartIndex + 1;

                            deviceStates[deviceLogin] = new DeviceState
                            {
                                PathPoints = orderedPoints,
                                CurrentIndex = randomStartIndex,
                                NextIndex = nextIndex,
                                Progress = 0,
                                SectorId = path.SectorId,
                                CurrentX = (float)orderedPoints[randomStartIndex].X,
                                CurrentY = (float)orderedPoints[randomStartIndex].Y,
                                LastUpdateTime = DateTimeOffset.UtcNow,
                                IsMovingForward = randomStartIndex != orderedPoints.Length - 1
                            };
                        }
                    }
                }

                // Update device positions and send location updates
                var now = DateTimeOffset.UtcNow;
                foreach (var (deviceLogin, state) in deviceStates)
                {
                    var currentPoint = state.PathPoints[state.CurrentIndex];
                    var nextPoint = state.PathPoints[state.NextIndex];

                    // Calculate time-based progress increment
                    var timeDelta = (now - state.LastUpdateTime).TotalSeconds;
                    state.Progress += MOVEMENT_STEP * (float)timeDelta;
                    state.LastUpdateTime = now;

                    // Check if we've reached the next point
                    if (state.Progress >= 1.0f)
                    {
                        // Move to next point
                        state.CurrentIndex = state.NextIndex;
                        state.Progress = 0;

                        // Determine next point based on direction
                        if (state.IsMovingForward)
                        {
                            if (state.CurrentIndex == state.PathPoints.Length - 1)
                            {
                                // Reached end, start moving backward
                                state.IsMovingForward = false;
                                state.NextIndex = state.CurrentIndex - 1;
                            }
                            else
                            {
                                state.NextIndex = state.CurrentIndex + 1;
                            }
                        }
                        else
                        {
                            if (state.CurrentIndex == 0)
                            {
                                // Reached start, start moving forward
                                state.IsMovingForward = true;
                                state.NextIndex = state.CurrentIndex + 1;
                            }
                            else
                            {
                                state.NextIndex = state.CurrentIndex - 1;
                            }
                        }

                        currentPoint = state.PathPoints[state.CurrentIndex];
                        nextPoint = state.PathPoints[state.NextIndex];
                    }

                    // Smooth interpolation using easing function
                    float easedProgress = EaseInOutQuad(state.Progress);
                    
                    // Interpolate position
                    state.CurrentX = (float)(currentPoint.X + (nextPoint.X - currentPoint.X) * easedProgress);
                    state.CurrentY = (float)(currentPoint.Y + (nextPoint.Y - currentPoint.Y) * easedProgress);

                    // Create and send location update
                    var locationContract = new DeviceLocationContract
                    {
                        Login = deviceLogin,
                        Locations = new[]
                        {
                            new LocationContract
                            {
                                SectorId = state.SectorId,
                                X = state.CurrentX,
                                Y = state.CurrentY,
                                IsMoving = true,
                                Interval = UPDATE_INTERVAL_MS,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            }
                        }
                    };

                    await writer.WriteAsync(locationContract);
                }

                // Wait before next update
                await Task.Delay(UPDATE_INTERVAL_MS);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error in DevicePathSimulatorFilter");
            }
        }

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2.0f * t * t : -1.0f + (4.0f - 2.0f * t) * t;
        }
    }
}
