using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;
using static ElPriceViewModel;

namespace ElPriceAPIDataAutoSave.Models.Tables
{
    public class ElPriceHistoryEntity : ITableEntity
    {
        public string RowKey { get; set; } = default!;
        public string PartitionKey { get; set; } = default!;
        public ETag ETag { get; set; } = ETag.All;
        public DateTimeOffset? Timestamp { get; set; }
        public string Date { get; set; } = default!;
        public string SE1Json { get; set; } = default!;
        public string SE2Json { get; set; } = default!;
        public string SE3Json { get; set; } = default!;
        public string SE4Json { get; set; } = default!;
    }
}
