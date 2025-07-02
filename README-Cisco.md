# Cisco Integration

The Cisco integration connects to Cisco DNA Spaces via HTTP stream to receive real-time location data from Cisco wireless infrastructure.

## Configuration

### Basic Configuration Structure

```json
{
  "tSync": {
    "Cisco": [
      {
        "HttpStreamOptions": {
          "Url": "https://partners.dnaspaces.eu/api/partners/v1/firehose/events",
          "Headers": {
            "X-API-Key": "your-api-key-here"
          },
          "ConnectionType": "http-stream",
          "TimeoutSeconds": 30,
          "RetryIntervalSeconds": 5
        },
        "Twinzo": {
          "TwinzoBaseUrl": "https://api.twinzo.com",
          "ClientGuid": "your-client-guid",
          "BranchGuid": "your-branch-guid",
          "ApiKey": "your-twinzo-api-key",
          "Timeout": 15000
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

#### HTTP Stream Options
- **Url**: Cisco DNA Spaces firehose API endpoint
- **Headers**: HTTP headers for authentication
  - **X-API-Key**: Your Cisco DNA Spaces API key
- **ConnectionType**: Connection type (typically "http-stream")
- **TimeoutSeconds**: HTTP request timeout in seconds
- **RetryIntervalSeconds**: Retry interval for failed connections

#### Twinzo Configuration
- **TwinzoBaseUrl**: Twinzo API base URL
- **ClientGuid**: Your Twinzo client GUID
- **BranchGuid**: Your Twinzo branch GUID
- **ApiKey**: Your Twinzo API key
- **Timeout**: API timeout in milliseconds

#### Performance Settings
- **RtlsSender.SendIntervalMillis**: How often to send data to Twinzo (milliseconds)
- **RtlsSender.MaxSize**: Maximum number of records to batch before sending

## Sector Configuration

Cisco sectors support optional coordinate offset configuration for coordinate system transformation.

### Sector Configuration Structure

```json
{
  "SectorId": "your-sector-id",
  "OffsetX": 0.0,
  "OffsetY": 0.0
}
```

### Offset Configuration

- **OffsetX**: X-axis offset in meters (optional)
- **OffsetY**: Y-axis offset in meters (optional)

## Data Flow

1. **HTTP Stream Connection**: Establishes persistent connection to Cisco DNA Spaces
2. **Data Reception**: Receives real-time location events via HTTP stream
3. **Data Parsing**: Parses Cisco-specific JSON event format
4. **Device Creation**: Automatically creates devices in Twinzo if they don't exist
5. **Coordinate Transformation**: Applies sector offsets if configured
6. **Area Computation**: Computes localization and no-go areas
7. **Data Transmission**: Sends processed data to Twinzo platform

## Supported Features

- Real-time location tracking via Cisco wireless infrastructure
- Automatic device creation and management
- Coordinate system transformation
- Area-based localization
- Connection resilience with automatic retry

## Prerequisites

- Cisco DNA Spaces account with API access
- Valid API key for Cisco DNA Spaces
- Network access to Cisco DNA Spaces API endpoints

## Troubleshooting

### Common Issues

1. **Connection failures**: Verify API key and network connectivity
2. **No data received**: Check Cisco DNA Spaces configuration
3. **Authentication errors**: Validate API key in headers
4. **Timeout issues**: Adjust timeout and retry settings

### Logging

Enable trace logging to debug data flow:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "tSync.Cisco": "Trace"
    }
  }
}
```

### Connection Resilience

The integration includes automatic retry logic:
- Reconnects automatically on connection loss
- Configurable retry intervals
- Exponential backoff for repeated failures 