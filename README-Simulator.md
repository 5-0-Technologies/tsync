# Simulator Integration

The Simulator integration generates synthetic location data for testing and development purposes, simulating device movement along predefined paths.

## Configuration

### Basic Configuration Structure

```json
{
  "tSync": {
    "Simulator": [
      {
        "UpdateInterval": 1000,
        "Paths": [
          {
            "PathId": 1,
            "Devices": ["device_login_1", "device_login_2", "device_login_3"]
          }
        ],
        "Twinzo": {
          "TwinzoBaseUrl": "https://api.platform.twinzo.eu/",
          "ClientGuid": "<CLIENT_GUID>",
          "BranchGuid": "<BRANCH_GUID>",
          "ApiKey": "<API_KEY>",
          "Timeout": 5000
        },
        "RtlsSender": {
          "SendIntervalMillis": 10000,
          "MaxSize": 50
        }
      }
    ]
  }
}
```

### Configuration Parameters

#### Simulator Settings
- **UpdateInterval**: Interval between location updates (milliseconds)
- **Paths**: Array of path configurations for device simulation

#### Path Configuration
- **PathId**: Unique identifier for the path
- **Devices**: Array of device login names to simulate on this path

#### Twinzo Configuration
- **TwinzoBaseUrl**: Twinzo API base URL
- **ClientGuid**: Your Twinzo client GUID
- **BranchGuid**: Your Twinzo branch GUID
- **ApiKey**: Your Twinzo API key
- **Timeout**: API timeout in milliseconds

#### Performance Settings
- **RtlsSender.SendIntervalMillis**: How often to send data to Twinzo (milliseconds)
- **RtlsSender.MaxSize**: Maximum number of records to batch before sending

## Data Flow

1. **Path Simulation**: Generates synthetic location data along predefined paths
2. **Device Simulation**: Simulates multiple devices moving along configured paths
3. **Location Updates**: Updates device positions at configured intervals
4. **Device Creation**: Automatically creates devices in Twinzo if they don't exist
5. **Area Computation**: Computes localization and no-go areas
6. **Data Transmission**: Sends simulated data to Twinzo platform

## Supported Features

- Synthetic location data generation
- Multiple device simulation
- Predefined path movement patterns
- Configurable update intervals
- Automatic device creation and management
- Area-based localization
- Testing and development support

## Use Cases

- **Development Testing**: Test Twinzo integration without real RTLS hardware
- **Performance Testing**: Generate high-volume location data for load testing
- **Demo Environments**: Create demonstration environments with simulated devices
- **Integration Testing**: Test data processing pipelines with controlled input

## Path Configuration Examples

### Single Path with Multiple Devices
```json
{
  "PathId": 1,
  "Devices": ["device_001", "device_002", "device_003"]
}
```

### Multiple Paths
```json
{
  "Paths": [
    {
      "PathId": 1,
      "Devices": ["device_001", "device_002"]
    },
    {
      "PathId": 2,
      "Devices": ["device_003", "device_004"]
    }
  ]
}
```

## Troubleshooting

### Common Issues

1. **No simulated data**: Check path configuration and device names
2. **High CPU usage**: Reduce update interval for better performance
3. **Device creation failures**: Verify Twinzo API credentials
4. **Path simulation errors**: Ensure path IDs are unique

### Logging

Enable trace logging to debug data flow:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "tSync.Simulator": "Trace"
    }
  }
}
```

### Performance Considerations

- Adjust `UpdateInterval` based on testing requirements
- Monitor system resources during high-volume simulation
- Configure appropriate `SendIntervalMillis` and `MaxSize` for Twinzo transmission
- Use multiple paths to simulate complex scenarios

### Development Best Practices

- Use descriptive device names for easy identification
- Configure realistic update intervals for your use case
- Test with small device counts before scaling up
- Monitor Twinzo API usage during simulation 