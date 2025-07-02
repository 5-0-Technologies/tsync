# Configuration
Program loads configuration from  **config.js** file pleaced in root directory of execution. Each tenant-branch uses own config record.
### Structure
```json
{
  "tenants": [
    {
      "TwinzoClientName": "name",
      "TwinzoClientGuid": "client registry guid from twinzo",
      "TwinzoBranchGuid": "branch registry guid from twinzo",
      "TwinzoApiKey": "server application registry key from twinzo",
      "DataSource": "data source specification",
      "SourceKey": "source key parameter depending on data source type"
    }
  ]
}
``` 

**DataSource types**
- quuppa: quuppa local udp stream
- simatic_udp: siemens simatic local tcp stream
- spin: old rtls potsgresql rtls

# Deployment
## Linux (Ubuntu specific details)
1. at first build program as **self contained** linux application.
2. copy publish folder to Linux machine
3. provide execute permissions on application root folder
```shell
chmod u+x ./tSync
```
4. execute application
```shell
./tSync
```

### systemd service
[More info](https://swimburger.net/blog/dotnet/how-to-run-a-dotnet-core-console-app-as-a-service-using-systemd-on-linux)  
Systemd expects all configuration files to be put under **'/etc/systemd/system/'**. 

1. Copy the service configuration file to **'/etc/systemd/system/appname.service'**
2. reload service binary
```shell
systemctl daemon-reload
```
3. run service
```shell
service ./tSync run
```
4. run service after restart
```shell
sudo systemctl enable tSync
```

Useful commands
```shell
sudo journalctl -u tSync -f  
sudo systemctl start tSync  
sudo systemctl stop tSync  
sudo systemctl restart tSync
```

#### Systemd service configuration file template
```yaml
[Service]
WorkingDirectory=/path/to/application/directory
ExecStart=/path/to/application/appname
Restart=always
StandardOutput=syslog
StandardError=syslog
SyslogIdentifier=appname
User=root

# Environment=NODE_ENV=production
Environment=DOTNET_ROOT=/usr/share/dotnet

[Install]
WantedBy=multi-user.target
```



## Container
### Manual pushing container to registry:
build image form Dockerfile: `docker build -t image_name .` [docs build](https://docs.docker.com/engine/reference/commandline/build/)  
tagging image: `docker tag [image_name] <IP:PORT/image_name:version>` [docs tag](https://docs.docker.com/engine/reference/commandline/tag/)   
tag example: `docker tag twinzosync 192.168.30.242:5000/twinzosync:latest `  
pushing to registry: `docker push <IP:PORT/image_name:tag>` [docs push](https://docs.docker.com/engine/reference/commandline/push/)   
push example: `docker push 192.168.30.242:5000/twinzosync:latest`
### Start container from registry
only pull images [docs pull](https://docs.docker.com/engine/reference/commandline/pull/):

```bash
    docker pull  <IP:PORT/image_name:tag>
    docker pull  192.168.30.242:5000/spintwinzosync:latest  
```

show images in local container image registry: `docker image ls`

download image and start container  [docs container run](https://docs.docker.com/engine/reference/commandline/container_run/):
```bash
docker container run -d -p 8800:8800 --restart always  --name twinzosync 192.168.30.242:5000/spintwinzosync:latest
```
    -d - detached
    -p - port
    --restart always - after crash container restart automatically 
    --name - local container name
    
### docker-compose
 Compose is a tool for defining and running multi-container Docker applications. With Compose, you use a YAML file to configure your application's services. Then, with a single command, you create and start all the services from your configuration.

A `docker-compose.yml` looks like this:
```yaml
    version: '3'
    services:
      twinzosync:
        container_name: twinzosync
        image: 192.168.30.242:5000/spintwinzosync:latest
        ports:
        - "8800:8800"
        networks:
          - administrator_mynet
        restart: always
    networks:
      administrator_mynet:
        external: 
           name: administrator_mynet
```
#### Start, stop and update containers with docker compose:  
start container and show every log: `docker-compose pull && docker-compose up` [docs compose up](https://docs.docker.com/compose/reference/up/)  
stopping container is with ^c  
start container in the background: `docker-compose pull && docker-compose up -d`  
stopping container: `docker-compose down`  
For updating container edit docker-compose.yml file or push new image to registry, after modification run:
`docker-compose pull && docker-compose up -d`

### Image registry
Show images in registry: http://192.168.30.242:5000/v2/_catalog  


Show image tags: http://192.168.30.242:5000/v2/spintwinzosync/tags/list

image path in container: `/var/lib/registry/docker/registry/v2/repositories/`  

# Quuppa
You have to log in to [quuppa customer portal](https://secure.quuppa.com/customerportal/login/form?logout).  
- [QPE_api_v2_2](https://secure.quuppa.com/customerportal/rfile/21)

# Cisco
Cisco DNA Spaces provides real-time location tracking through their Firehose API. tSync can connect to this HTTP stream to receive location updates and forward them to the Twinzo platform.

## Configuration
To configure Cisco DNA Spaces integration, add a `Cisco` section to your configuration:

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
        }
      }
    ]
  }
}
```

## How It Works
1. tSync connects to the Cisco Firehose API using HTTP streaming
2. Real-time IOT_TELEMETRY events are received and parsed
3. Dynamic sector mapping uses the `mapId` from the stream to find the corresponding Twinzo sector
4. Location data is transformed to Twinzo format using sector coordinates
5. Devices are automatically created in Twinzo using MAC addresses as login identifiers
6. Location updates are sent to Twinzo platform

## Dynamic Sector Mapping
Cisco uses dynamic sector mapping based on the `mapId` field in the stream:

- **Automatic Sector Detection**: The `mapId` from each location event is used to find the corresponding Twinzo sector
- **Sector Configuration**: Configure sectors in Twinzo with the `Cisco` provider configuration containing the `mapId`
- **Coordinate Transformation**: Uses the sector's coordinate system and applies any configured offsets

## Data Format
Cisco Firehose sends IOT_TELEMETRY events in the following format:
```json
{
  "recordUid": "event-6c654b0b",
  "recordTimestamp": 1750351812337,
  "eventType": "IOT_TELEMETRY",
  "iotTelemetry": {
    "deviceInfo": {
      "deviceMacAddress": "29:2f:2b:c5:fa:48"
    },
    "detectedPosition": {
      "xPos": 26.6,
      "yPos": 62.3,
      "latitude": 50.049870563654046,
      "longitude": 14.434983926990684,
      "mapId": "03c4bea7afd3b16d58f153db346d8070"
    }
  }
}
```

## Device Association
To associate incoming Cisco data with devices in Twinzo:
1. Create a new device in Twinzo Portal > Accounts > Devices
2. Set the Login field to the MAC address of the Cisco device
3. tSync will automatically match incoming data to existing devices

## Sector Configuration
Configure sectors in Twinzo with Cisco provider settings:
```json
{
  "Cisco": {
    "SectorId": "03c4bea7afd3b16d58f153db346d8070",
    "OffsetX": 0,
    "OffsetY": 0
  }
}
```

## Testing
You can test the Cisco Firehose connection using curl:
```bash
curl -s "https://partners.dnaspaces.eu/api/partners/v1/firehose/events" -H "X-API-Key: your-api-key"
```
