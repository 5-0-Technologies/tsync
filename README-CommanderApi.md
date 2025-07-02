# Commander API Integration

The Commander API integration polls a Commander API endpoint to retrieve GPS location data for vehicles and equipment, providing real-time tracking capabilities.

## Configuration

### Basic Configuration Structure

```json
{
  "tSync": {
    "CommanderApi": [
      {
        "ApiBaseUrl": "https://commander-api.example.com/v1",
        "Username": "<USERNAME>",
        "Password": "<PASSWORD>",
        "PollIntervalMillis": 10000,
        "VehicleNameUpdateIntervalMillis": 600000,
        "Twinzo": {
          "TwinzoBaseUrl": "https://api.platform.twinzo.eu/",
          "ClientGuid": "<CLIENT_GUID>",
          "BranchGuid": "<BRANCH_GUID>",
          "SectorId": "<SECTOR_ID>",
          "ApiKey": "<API_KEY>",
          "Timeout": 5000
        },
        "RtlsSender": {
          "SendIntervalMillis": 10000,
          "MaxSize": 50
        },
        "Channel": {
          "Capacity": 1000
        },
        "MemoryCache": {
          "ExpirationInSeconds": 60
        }
      }
    ]
  }
}
```

### Configuration Parameters

#### API Configuration
- **ApiBaseUrl**: Base URL of the Commander API
- **Username**: Basic Authentication username
- **Password**: Basic Authentication password
- **PollIntervalMillis**: How often to poll the API (milliseconds)
- **VehicleNameUpdateIntervalMillis**: How often to update vehicle names (milliseconds, default: 600000)

#### Twinzo Configuration
- **TwinzoBaseUrl**: Twinzo API base URL
- **ClientGuid**: Your Twinzo client GUID
- **BranchGuid**: Your Twinzo branch GUID
- **SectorId**: Twinzo sector ID for the GPS data
- **ApiKey**: Your Twinzo API key
- **Timeout**: API timeout in milliseconds

#### Performance Settings
- **RtlsSender.SendIntervalMillis**: How often to send data to Twinzo (milliseconds)
- **RtlsSender.MaxSize**: Maximum number of records to batch before sending
- **Channel.Capacity**: Size of the channel buffer (0 for unlimited)
- **MemoryCache.ExpirationInSeconds**: Cache expiration for Twinzo data (seconds)

## Data Flow

1. **API Polling**: Periodically polls Commander API for GPS data
2. **Authentication**: Uses Basic Authentication for API access
3. **Data Parsing**: Parses Commander-specific GPS data format
4. **Vehicle Name Updates**: Periodically updates vehicle names from API
5. **GPS to Sector Conversion**: Converts GPS coordinates to sector coordinates
6. **Device Creation**: Automatically creates devices in Twinzo if they don't exist
7. **Area Computation**: Computes localization and no-go areas
8. **Data Transmission**: Sends processed data to Twinzo platform

## Supported Features

- GPS coordinate processing and conversion
- Vehicle name management and updates
- Automatic device creation and management
- Area-based localization
- Basic Authentication for API security
- Configurable polling intervals
- Memory caching for performance optimization

## Prerequisites

- Commander API endpoint with GPS data
- Valid API credentials (username/password)
- Network access to Commander API
- GPS-enabled vehicles or equipment

## Commander API Requirements

The Commander API should provide:
- GPS location data for vehicles/equipment
- Vehicle identification information
- Real-time or near-real-time data updates
- Basic Authentication support
- JSON response format

## Troubleshooting

### Common Issues

1. **API connection failures**: Verify API URL and network connectivity
2. **Authentication errors**: Check username and password credentials
3. **No data received**: Verify API endpoint and data format
4. **GPS conversion errors**: Check sector configuration in Twinzo

### Logging

Enable trace logging to debug data flow:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "tSync.CommanderApi": "Trace"
    }
  }
}
```

### Performance Tuning

- Adjust `PollIntervalMillis` based on data update frequency requirements
- Configure `VehicleNameUpdateIntervalMillis` based on vehicle name change frequency
- Tune `SendIntervalMillis` and `MaxSize` for optimal Twinzo transmission
- Set appropriate `Channel.Capacity` based on data volume

### API Rate Limiting

- Monitor API response times and adjust polling intervals
- Implement exponential backoff for failed requests
- Consider API rate limits when configuring polling frequency 