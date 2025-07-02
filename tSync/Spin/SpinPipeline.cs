using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;
using tSync.Filters;
using tSync.Model;
using tSync.Spin.Filters;
using tSync.Spin.Models;
using tSync.Spin.Options;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.InputOutput;

namespace tSync.Spin
{
    public class SpinPipeline : Pipeline
    {
        private readonly SpinPipelineOptions opt;
        private readonly ILogger logger;

        public SpinPipeline(SpinPipelineOptions pipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = pipelineOptions;
            logger = loggerFactory.CreateLogger(Providers.Spin);
        }

        public override void Register(ICollection<Filter> filters)
        {
            logger.LogTrace($"{GetType().Name} -> Register");
            logger.LogInformation(opt.ToString());

            ConnectionOptionsBuilder optionsBuilder = new ConnectionOptionsBuilder();
            ConnectionOptions connectionOptions = optionsBuilder
                .Url(opt.Twinzo.TwinzoBaseUrl)
                .Client("Infotech")
                .ClientGuid(opt.Twinzo.ClientGuid.ToString())
                .BranchGuid(opt.Twinzo.BranchGuid.ToString())
                .ApiKey(opt.Twinzo.ApiKey)
                .Timeout(opt.Twinzo.Timeout)
                .Version(ConnectionOptions.VERSION_3)
                .Build();

            var connectorV3 = (DevkitConnectorV3)DevkitFactory.CreateDevkitConnector(connectionOptions);
            
            var memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromSeconds(10)
            });
            var cacheConnector = new DevkitCacheConnector(connectorV3, memoryCache);
            cacheConnector.ExpirationInSeconds = opt.MemoryCache.ExpirationInSeconds;

            var selectCommand = Sql.GetLocalization;

            // Channels
            Channel<DataRow> postgreChannel;
            Channel<SpinLocationData> spinChannel;
            Channel<DeviceLocationContract> locationChannel;
            if (opt.Channel.Capacity < 1)
            {
                postgreChannel = Channel.CreateUnbounded<DataRow>();
                spinChannel = Channel.CreateUnbounded<SpinLocationData>();
                locationChannel = Channel.CreateUnbounded<DeviceLocationContract>();
            }
            else
            {
                postgreChannel = Channel.CreateBounded<DataRow>(opt.Channel.Capacity);
                spinChannel = Channel.CreateBounded<SpinLocationData>(opt.Channel.Capacity);
                locationChannel = Channel.CreateBounded<DeviceLocationContract>(opt.Channel.Capacity);
            }       

            var postgreFilter = new PostgreFilter(postgreChannel.Writer, opt.SpinConnectionString, selectCommand);
            var timerFiler = new TimerFilter(postgreFilter, opt.SpinScanIntervalMillis, 1, 1);

            var transformFilter = new TransformChannelFilter<DataRow, SpinLocationData>(postgreChannel.Reader, spinChannel.Writer, Transform);
            var locationFilter = new SpinLocationTransformFilter(spinChannel.Reader, locationChannel.Writer, cacheConnector, opt.Twinzo.BranchGuid, opt.SpinScanIntervalMillis);
            var areaFilter = new AreaFilter(locationChannel.Reader, locationChannel.Writer, cacheConnector);
            var rtlsFilter = new RtlsSenderFilter(locationChannel.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            filters.Add(timerFiler);
            filters.Add(transformFilter);
            filters.Add(locationFilter);
            filters.Add(areaFilter);
            filters.Add(rtlsFilter);

            postgreFilter.Logger = logger;
            foreach (var filter in filters)
            {
                filter.Logger = logger;
            }
        }

        public SpinLocationData Transform(DataRow dataRow)
        {
            return ToObject<SpinLocationData>(dataRow);
        }

        public static T ToObject<T>(DataRow dataRow) where T : new()
        {
            T item = new T();

            foreach (DataColumn column in dataRow.Table.Columns)
            {
                PropertyInfo property = GetProperty(typeof(T), column.ColumnName);

                if (property != null && dataRow[column] != DBNull.Value && dataRow[column].ToString() != "NULL")
                {
                    property.SetValue(item, ChangeType(dataRow[column], property.PropertyType), null);
                }
            }

            return item;
        }

        private static PropertyInfo GetProperty(Type type, string attributeName)
        {
            PropertyInfo property = type.GetProperty(attributeName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property != null)
            {
                return property;
            }

            return type.GetProperties()
                 .Where(p => p.IsDefined(typeof(DisplayAttribute), false) && p.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name == attributeName)
                 .FirstOrDefault();
        }

        public static object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }

            return Convert.ChangeType(value, type);
        }
    }
}
