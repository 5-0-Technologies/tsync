# Precog Integration

The Precog integration processes location data from Azure Event Hubs and Microsoft SQL Server databases, performing advanced trilateration calculations for precise location tracking.

## Configuration

### Basic Configuration Structure

```json
{
  "tSync": {
    "Precog": [
      {
        "ConsumerGroup": "<CONSUMER_GROUP>",
        "EventHubConnectionString": "<EVENT_HUB_CONNECTION_STRING>",
        "MsSqlConnectionString": "<MSXQL_CONNECTION_STRING>",
        "AggregationIntervalMillis": 10000,
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

#### Azure Event Hub Configuration
- **ConsumerGroup**: Event Hub consumer group name
- **EventHubConnectionString**: Azure Event Hub connection string
- **MsSqlConnectionString**: Microsoft SQL Server connection string for beacon data

#### Performance Settings
- **AggregationIntervalMillis**: Data aggregation interval (milliseconds)
- **RtlsSender.SendIntervalMillis**: How often to send data to Twinzo (milliseconds)
- **RtlsSender.MaxSize**: Maximum number of records to batch before sending

#### Twinzo Configuration
- **TwinzoBaseUrl**: Twinzo API base URL
- **ClientGuid**: Your Twinzo client GUID
- **BranchGuid**: Your Twinzo branch GUID
- **ApiKey**: Your Twinzo API key
- **Timeout**: API timeout in milliseconds

## Data Flow

1. **Event Hub Connection**: Connects to Azure Event Hub for real-time data ingestion
2. **SQL Server Connection**: Retrieves beacon configuration from SQL Server
3. **Data Aggregation**: Aggregates location data over configured intervals
4. **Trilateration**: Performs advanced trilateration calculations for precise positioning
5. **Data Transformation**: Converts Precog data format to Twinzo format
6. **Device Creation**: Automatically creates devices in Twinzo if they don't exist
7. **Area Computation**: Computes localization and no-go areas
8. **Data Transmission**: Sends processed data to Twinzo platform

## Supported Features

- Azure Event Hub integration for real-time data ingestion
- Microsoft SQL Server integration for beacon configuration
- Advanced trilateration algorithms for precise positioning
- Data aggregation and filtering
- Automatic device creation and management
- Area-based localization
- Real-time location tracking with high accuracy

## Prerequisites

- Azure Event Hub with configured consumer group
- Microsoft SQL Server database with beacon configuration
- Valid connection strings for both Event Hub and SQL Server
- Network access to Azure services and SQL Server

## Azure Event Hub Configuration

Ensure your Event Hub is configured with:
- Proper access policies and connection strings
- Consumer group for tSync application
- Event serialization format compatible with Precog data

## SQL Server Database Requirements

The SQL Server database should contain:
- Beacon configuration tables
- Location reference data
- Calibration parameters for trilateration

## Troubleshooting

### Common Issues

1. **Event Hub connection failures**: Verify connection string and access policies
2. **SQL Server connection errors**: Check connection string and network access
3. **Data processing errors**: Validate beacon configuration in SQL Server
4. **Trilateration accuracy issues**: Review beacon placement and calibration

### Logging

Enable trace logging to debug data flow:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "tSync.Precog": "Trace"
    }
  }
}
```

### Performance Optimization

- Adjust `AggregationIntervalMillis` based on data volume and accuracy requirements
- Configure Event Hub consumer group for optimal throughput
- Tune SQL Server connection pooling for database performance
- Monitor Event Hub throughput and adjust consumer group settings

### Azure Service Dependencies

- Event Hub namespace with proper access policies
- Consumer group with appropriate permissions
- Network connectivity to Azure services
- Proper authentication and authorization setup 