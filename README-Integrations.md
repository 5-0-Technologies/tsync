# tSync Integrations

tSync provides integrations with multiple Real-Time Location System (RTLS) providers to synchronize location data with the Twinzo platform. Each integration is designed to handle provider-specific data formats and communication protocols.

## Available Integrations

### [Quuppa Integration](README-Quuppa.md)
- **Protocols**: UDP, MQTT
- **Features**: Real-time location tracking, battery monitoring, panic button detection
- **Use Case**: Indoor positioning with Quuppa RTLS systems
- **Key Configuration**: Coordinate offsets, MQTT/UDP settings

### [Cisco Integration](README-Cisco.md)
- **Protocols**: HTTP Stream
- **Features**: Cisco DNA Spaces integration, wireless infrastructure tracking
- **Use Case**: Enterprise wireless location tracking
- **Key Configuration**: API key, HTTP stream settings

### [ThingPark Integration](README-ThingPark.md)
- **Protocols**: HTTP POST
- **Features**: GPS coordinate processing, Actility ThingPark Location integration
- **Use Case**: GPS-based outdoor tracking
- **Key Configuration**: HTTP server settings, GPS conversion

### [RFControls Integration](README-RFControls.md)
- **Protocols**: MQTT
- **Features**: RFID tag tracking, region-based filtering
- **Use Case**: RFID-based asset tracking
- **Key Configuration**: MQTT settings, region configuration

### [Precog Integration](README-Precog.md)
- **Protocols**: Azure Event Hub, Microsoft SQL Server
- **Features**: Advanced trilateration, high-accuracy positioning
- **Use Case**: Precision indoor positioning
- **Key Configuration**: Event Hub connection, SQL Server settings

### [Commander API Integration](README-CommanderApi.md)
- **Protocols**: HTTP REST API
- **Features**: GPS vehicle tracking, Basic Authentication
- **Use Case**: Fleet management and vehicle tracking
- **Key Configuration**: API credentials, polling intervals

### [Simulator Integration](README-Simulator.md)
- **Protocols**: Synthetic data generation
- **Features**: Path-based simulation, multiple device support
- **Use Case**: Testing and development
- **Key Configuration**: Path definitions, update intervals

## Common Configuration Elements

All integrations share common configuration elements:

### Global Settings
```json
{
  "tSync": {
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
}
```

### Twinzo Configuration
All integrations require Twinzo API configuration:
```json
{
  "Twinzo": {
    "TwinzoBaseUrl": "https://api.platform.twinzo.eu/",
    "ClientGuid": "<CLIENT_GUID>",
    "BranchGuid": "<BRANCH_GUID>",
    "ApiKey": "<API_KEY>",
    "Timeout": 5000
  }
}
```

## Integration Selection Guide

| Use Case | Recommended Integration | Key Benefits |
|----------|------------------------|--------------|
| Indoor positioning | Quuppa | High accuracy, battery monitoring |
| Enterprise wireless | Cisco | Infrastructure integration |
| Outdoor GPS tracking | ThingPark | GPS coordinate processing |
| Asset tracking | RFControls | RFID-based tracking |
| Precision positioning | Precog | Advanced trilateration |
| Fleet management | Commander API | Vehicle tracking |
| Testing/Development | Simulator | Synthetic data generation |

## Configuration Best Practices

1. **Start with one integration**: Begin with a single integration to understand the configuration process
2. **Use example configurations**: Each integration includes example configuration files
3. **Test with simulator**: Use the simulator integration for initial testing
4. **Monitor performance**: Adjust intervals and batch sizes based on data volume
5. **Secure credentials**: Store sensitive information in environment variables or secure configuration stores

### Common Issues Across Integrations

1. **No data received**: Check network connectivity and provider configuration
2. **Authentication errors**: Verify API credentials and permissions
3. **Coordinate misalignment**: Review sector configuration and offsets
4. **Performance issues**: Adjust intervals and batch sizes

### Logging Configuration

Enable detailed logging for debugging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "tSync": "Trace"
    }
  }
}
```

## Getting Started

1. Choose the integration that matches your RTLS provider
2. Review the specific integration README for detailed configuration
3. Configure your `appSettings.json` with the required settings
4. Set up sector configuration in Twinzo
5. Test the integration with a small number of devices
6. Monitor logs and adjust configuration as needed

## Support

For integration-specific issues, refer to the individual README files. For general tSync questions, consult the main project documentation. 