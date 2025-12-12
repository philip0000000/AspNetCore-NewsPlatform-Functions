using ElPriceAPIDataAutoSave.Models.Tables;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using static ElPriceViewModel;

namespace ElPriceAPIDataAutoSave.Services
{
    public class ElPriceHistoryService : IElPriceHistoryService
    {
        public readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _tableName = "ElPriceHistory";
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;

        public ElPriceHistoryService(IConfiguration config)
        {
            _configuration = config;
            _connectionString = _configuration["TableStorageConnection"]
                ?? throw new ArgumentNullException("TableStorageConnection connection string is not configured.");

            _tableServiceClient = new TableServiceClient(_connectionString);
            _tableClient = _tableServiceClient.GetTableClient(_tableName);
        }

        public async Task EnsureTableAsync() =>
            await _tableClient.CreateIfNotExistsAsync();
        
        public async Task AddSnapshotAsync(ElPriceViewModel dto)
        {
            Console.WriteLine(">>> AddSnapshotAsync CALLED");
            try
            {
                var createResponse = await _tableClient.CreateIfNotExistsAsync();
                Console.WriteLine(">>> CreateIfNotExistsAsync: " + (createResponse == null ? "Table already exists" : "Table created"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> ERROR creating table: " + ex);
                throw;
            }
            try
            {
                ElPriceHistoryEntity entity = new ElPriceHistoryEntity
                {
                    PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
                    RowKey = $"{Guid.NewGuid():N}-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    SE1Json = JsonSerializer.Serialize(dto.SE1),
                    SE2Json = JsonSerializer.Serialize(dto.SE2),
                    SE3Json = JsonSerializer.Serialize(dto.SE3),
                    SE4Json = JsonSerializer.Serialize(dto.SE4),
                };
                await _tableClient.AddEntityAsync(entity);
                Console.WriteLine(">>> ENTITY ADDED OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> ERROR ADDING ENTITY: " + ex);
                throw;
            }
        }

        public async Task<IEnumerable<ElPriceViewModel>> GetLatestSnapshotsAsync(int count = 10)
        {
            await EnsureTableAsync();

            // Query all partitions (or optionally filter by recent dates)
            var query = _tableClient.QueryAsync<ElPriceHistoryEntity>(
                filter: "PartitionKey ne ''"
            );
            // Collect entities
            var entities = new List<ElPriceHistoryEntity>();
            await foreach (var entity in query)
            {
                entities.Add(entity);
            }
            // Sort descending by RowKey (timestamp) and take top N
            var latestEntities = entities
                .OrderByDescending(e => e.RowKey)
                .Take(count);
            // Map to ElPriceViewModel and deserialize JSON
            var snapshots = latestEntities.Select(e => new ElPriceViewModel
            {
                Date = e.Date,
                SE1 = JsonSerializer.Deserialize<SEItem[]>(e.SE1Json)!,
                SE2 = JsonSerializer.Deserialize<SEItem[]>(e.SE2Json)!,
                SE3 = JsonSerializer.Deserialize<SEItem[]>(e.SE3Json)!,
                SE4 = JsonSerializer.Deserialize<SEItem[]>(e.SE4Json)!,
            });
            return snapshots;
        }

        public async Task <IEnumerable<ElPriceViewModel>> GetSnapshotsByDateAsync(string date)
        {
            await EnsureTableAsync();

            // PartitionKey is formatted as yyyyMMdd
            string partitionKey = DateTime.Parse(date).ToString("yyyyMMdd");

            // Query entities for the given partition (date)
            var query = _tableClient.QueryAsync<ElPriceHistoryEntity>(
                filter: $"PartitionKey eq '{partitionKey}'"
            );

            var entities = new List<ElPriceHistoryEntity>();
            await foreach (var entity in query)
            {
                entities.Add(entity);
            }

            // Sort by RowKey (timestamp) ascending or descending
            var sortedEntities = entities.OrderBy(e => e.RowKey);

            // Map to ElPriceViewModel and deserialize JSON
            var snapshots = sortedEntities.Select(e => new ElPriceViewModel
            {
                Date = e.Date,
                SE1 = JsonSerializer.Deserialize<SEItem[]>(e.SE1Json)!,
                SE2 = JsonSerializer.Deserialize<SEItem[]>(e.SE2Json)!,
                SE3 = JsonSerializer.Deserialize<SEItem[]>(e.SE3Json)!,
                SE4 = JsonSerializer.Deserialize<SEItem[]>(e.SE4Json)!,
            });

            return snapshots;
        }
    }
}
