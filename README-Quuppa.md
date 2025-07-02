# Quuppa Integration

The Quuppa integration provides real-time location tracking by connecting to Quuppa RTLS systems via UDP and/or MQTT protocols.

## Configuration

### Basic Configuration Structure

```json
{
  "tSync": {
    "Quuppa": [
      {
        "UdpOptions": {
          "Port": 9050
        },
        "MqttOptions": {
          "UseWebSockets": true,
          "Host": "<MQTT_HOST>",
          "Port": 443,
          "Username": "<MQTT_USERNAME>",
          "Password": "<MQTT_PASSWORD>",
          "Topic": "<MQTT_TOPIC>"
        },
        "Twinzo": {
          "TwinzoBaseUrl": "https://api.platform.twinzo.eu/",
          "ClientGuid": "<CLIENT_GUID>",
          "BranchGuid": "<BRANCH_GUID>",
          "ApiKey": "<API_KEY>",
          "Timeout": 5000
        },
        "QuuppaScanIntervalMillis": 1000,
        "AggregationIntervalMillis": 10000,
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

#### UDP Options
- **Port**: UDP port to listen for Quuppa data (default: 9050)

#### MQTT Options
- **UseWebSockets**: Set to `true` for WebSocket connection, `false` for TCP
- **Host**: MQTT broker hostname or IP address
- **Port**: MQTT broker port (443 for WebSocket, 1883 for TCP)
- **Username**: MQTT authentication username
- **Password**: MQTT authentication password
- **Topic**: MQTT topic to subscribe to

#### Twinzo Configuration
- **TwinzoBaseUrl**: Twinzo API base URL
- **ClientGuid**: Your Twinzo client GUID
- **BranchGuid**: Your Twinzo branch GUID
- **ApiKey**: Your Twinzo API key
- **Timeout**: API timeout in milliseconds

#### Performance Settings
- **QuuppaScanIntervalMillis**: Interval between Quuppa data scans (milliseconds)
- **AggregationIntervalMillis**: Data aggregation interval (milliseconds)
- **RtlsSender.SendIntervalMillis**: How often to send data to Twinzo (milliseconds)
- **RtlsSender.MaxSize**: Maximum number of records to batch before sending

## Sector Configuration

Quuppa sectors require coordinate offset configuration to transform Quuppa coordinates to your facility's coordinate system.

### Sector Configuration Structure

```json
{
  "SectorId": "your-sector-id",
  "OffsetX": 0.0,
  "OffsetY": 0.0,
  "OffsetZ": 0.0
}
```

### Offset Configuration

- **OffsetX**: X-axis offset in millimeters (positive values move right)
- **OffsetY**: Y-axis offset in millimeters (positive values move up)
- **OffsetZ**: Z-axis offset in millimeters (positive values move forward)

### Coordinate Transformation

The integration automatically applies the following transformations:
- X coordinate: `OffsetX + (QuuppaX * 1000)`
- Y coordinate: `OffsetY + SectorHeight - (QuuppaY * 1000)`
- Z coordinate: `OffsetZ + (QuuppaZ * 1000)`

## Data Flow

1. **Data Reception**: Receives location data via UDP or MQTT
2. **Data Parsing**: Parses Quuppa-specific data format
3. **Device Creation**: Automatically creates devices in Twinzo if they don't exist
4. **Coordinate Transformation**: Applies sector offsets and coordinate system conversion
5. **Panic Button Detection**: Processes panic button events
6. **Area Computation**: Computes localization and no-go areas
7. **Data Transmission**: Sends processed data to Twinzo platform

## Supported Features

- Real-time location tracking
- Battery level monitoring
- Movement status detection
- Panic button event processing
- Automatic device creation
- Coordinate system transformation
- Area-based localization

## Troubleshooting

### Common Issues

1. **No data received**: Check UDP port or MQTT connection settings
2. **Coordinate misalignment**: Verify sector offset configuration
3. **Device not created**: Ensure Twinzo API credentials are correct
4. **High latency**: Adjust scan and aggregation intervals

### Logging

Enable trace logging to debug data flow:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "tSync.Quuppa": "Trace"
    }
  }
}
``` 