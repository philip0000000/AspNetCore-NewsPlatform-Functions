using Azure;
using Azure.Data.Tables;

namespace AFHPNewsFunctions.Entities
{
    public class CurrencyEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public double Value { get; set; }
    }
}
