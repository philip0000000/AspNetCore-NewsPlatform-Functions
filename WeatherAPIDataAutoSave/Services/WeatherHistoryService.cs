using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure;
using Microsoft.Extensions.Configuration;
using WeatherAPIDataAutoSave.Models;
using WeatherAPIDataAutoSave.Tables;

namespace WeatherAPIDataAutoSave.Services
{
    public class WeatherHistoryService : IWeatherHistoryService
    {
        private readonly IConfiguration _configuration;
        private readonly string _tableName = "WeatherHistory";
        private readonly Lazy<TableClient> _lazyTableClient;

        public WeatherHistoryService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _lazyTableClient = new Lazy<TableClient>(InitializeTableClient);
        }

        private TableClient TableClient => _lazyTableClient.Value;

        private TableClient InitializeTableClient()
        {
            var connectionString = _configuration["TableStorageConnection"]
                ?? throw new InvalidOperationException("TableStorageConnection is not configured.");

            var serviceClient = new TableServiceClient(connectionString);
            var client = serviceClient.GetTableClient(_tableName);
            client.CreateIfNotExists();
            return client;
        }

        public async Task AddSnapshotAsync(string city, WeatherForecastVM forecast)
        {
            var entity = new WeatherHistoryEntity
            {
                PartitionKey = $"Weather-{city}",
                RowKey = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                City = city,
                TemperatureC = forecast.TemperatureC,
                Humidity = forecast.Humidity,
                WindSpeed = forecast.WindSpeed,
                IconUrl = forecast.Icon.Url,
                IconCode = forecast.Icon.Code,
                CreatedUtc = DateTime.UtcNow
            };

            await TableClient.AddEntityAsync(entity);
        }

        public async Task<IReadOnlyList<WeatherHistoryEntity>> GetHistoryAsync(string city, DateTime from, DateTime to)
        {
            var pk = $"Weather-{city}";
            var filter = $"PartitionKey eq '{pk}' and Timestamp ge datetime'{from:O}' and Timestamp le datetime'{to:O}'";

            var results = new List<WeatherHistoryEntity>();
            await foreach (var entity in TableClient.QueryAsync<TableEntity>(filter))
            {
                results.Add(new WeatherHistoryEntity
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Timestamp = entity.Timestamp,
                    ETag = entity.ETag,
                    City = entity.GetString("City") ?? "",
                    TemperatureC = entity.TryGetValue("TemperatureC", out var tempObj) ? Convert.ToDecimal(tempObj) : 0,
                    Humidity = entity.TryGetValue("Humidity", out var humObj) ? Convert.ToDecimal(humObj) : 0,
                    WindSpeed = entity.TryGetValue("WindSpeed", out var windObj) ? Convert.ToDecimal(windObj) : 0,
                    IconUrl = entity.GetString("IconUrl") ?? "",
                    IconCode = entity.GetString("IconCode") ?? "",
                    CreatedUtc = entity.GetDateTime("CreatedUtc") ?? DateTime.MinValue
                });
            }

            return results.OrderBy(r => r.CreatedUtc).ToList();
        }
    }
}

