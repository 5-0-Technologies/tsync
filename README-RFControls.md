# RFControls Integration

The RFControls integration connects to RFControls RTLS systems via MQTT to receive real-time location data from RFID tags and readers.

## Configuration

### Basic Configuration Structure

```json
{
  "tSync": {
    "RFControls": [
      {
        "ScanIntervalMillis": 1000,
        "Regions": ["<REGION>"],
        "ReceiveInterval": 1000,
        "Mqtt": {
          "WithAutoReconnectDelay": "00:00:10",
          "TcpServer": "<TCP_SERVER>",
          "Port": 1883,
          "User": "<USER>",
          "Password": "<ADMIN>"
        },
        "Twinzo": {
          "TwinzoBaseUrl": "https://api.platform.twinzo.eu/",
          "ClientGuid": "<CLIENT_GUID>",
          "BranchGuid": "<BRANCH_GUID>",
          "ApiKey": "<API_KEY>",
          "Timeout": 5000
        },
        "RtlsSender": {
          "SendIntervalMillis": 1000,
          "MaxSize": 50
        }
      }
    ]
  }
}
```

### Configuration Parameters

#### MQTT Options
- **WithAutoReconnectDelay**: Automatic reconnection delay (format: "HH:MM:SS")
- **TcpServer**: MQTT broker hostname or IP address
- **Port**: MQTT broker port (default: 1883)
- **User**: MQTT authentication username
- **Password**: MQTT authentication password

#### Region Configuration
- **Regions**: Array of RFControls region identifiers to monitor
- **ScanIntervalMillis**: Interval between tag scans (milliseconds)
- **ReceiveInterval**: Interval for receiving MQTT messages (milliseconds)

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

RFControls sectors require minimal configuration for basic sector identification.

### Sector Configuration Structure

```json
{
  "SectorId": "your-sector-id"
}
```

## Data Flow

1. **MQTT Connection**: Establishes connection to RFControls MQTT broker
2. **Data Reception**: Receives tag blink data via MQTT
3. **Data Parsing**: Parses RFControls-specific message format
4. **Region Filtering**: Filters data by configured regions
5. **Device Creation**: Automatically creates devices in Twinzo if they don't exist
6. **Area Computation**: Computes localization and no-go areas
7. **Data Transmission**: Sends processed data to Twinzo platform

## Supported Features

- Real-time RFID tag tracking
- Region-based filtering
- Automatic device creation and management
- Area-based localization
- MQTT-based communication with automatic reconnection
- Tag blink event processing

## Prerequisites

- RFControls RTLS system with MQTT broker
- Valid MQTT credentials
- Network access to RFControls MQTT broker
- Configured regions in RFControls system

## RFControls System Configuration

Ensure your RFControls system is configured to:
- Publish tag blink events to MQTT
- Use the configured MQTT broker
- Include region information in messages
- Provide proper authentication credentials

## Troubleshooting

### Common Issues

1. **MQTT connection failures**: Verify broker address, port, and credentials
2. **No data received**: Check region configuration and MQTT topics
3. **Authentication errors**: Validate MQTT username and password
4. **Region filtering issues**: Ensure regions match RFControls configuration

### Logging

Enable trace logging to debug data flow:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "tSync.RFControls": "Trace"
    }
  }
}
```

### MQTT Connection Resilience

The integration includes automatic reconnection:
- Reconnects automatically on connection loss
- Configurable reconnection delay
- Maintains subscription to topics after reconnection

### Performance Tuning

- Adjust `ScanIntervalMillis` based on tag update frequency
- Configure `ReceiveInterval` based on MQTT message volume
- Tune `SendIntervalMillis` and `MaxSize` for optimal Twinzo transmission 