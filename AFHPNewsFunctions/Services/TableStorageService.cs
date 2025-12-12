using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AFHPNewsFunctions.Services
{
    public class TableStorageService : ITableStorageService
    {
        private const string TABLE = "CurrencyHistory";

        private readonly ILogger<TableStorageService> _logger;
        private string? _connectionString = null;
        
        public TableStorageService(
            IConfiguration configuration,
            ILogger<TableStorageService> logger
        )
        {
            _logger = logger;

            _connectionString = configuration.GetValue<string>("AzureWebJobsStorage");
        }

        public async Task<bool> Add(List<ITableEntity> entities)
        {
            if (_connectionString == null)
                return false;

            var client = new TableClient(_connectionString, TABLE);
            await client.CreateIfNotExistsAsync();

            var success = true;
            foreach (var entity in entities)
            {
                try
                {
                    await client.AddEntityAsync(entity);
                }
                catch (RequestFailedException ex)
                {
                    _logger.LogError("Add operation failed. Status code: {status}", ex.Status);
                    success = false;
                }
            }

            return success;
        }

        // All entities must have same partition key!
        // Fails all if one fails
        public async Task<bool> AddBatch(List<ITableEntity> entities)
        {
            if (_connectionString == null)
                return false;

            var client = new TableClient(_connectionString, TABLE);
            await client.CreateIfNotExistsAsync();
            
            var batch = new List<TableTransactionAction>();
            foreach (var entity in entities)
                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));

            try
            {
                await client.SubmitTransactionAsync(batch);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError("Batch operation failed. Status code: {status}", ex.Status);

                return false;
            }

            return true;
        }
    }
}
