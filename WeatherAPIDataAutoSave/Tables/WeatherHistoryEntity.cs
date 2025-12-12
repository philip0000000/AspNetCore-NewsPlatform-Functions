using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace WeatherAPIDataAutoSave.Tables
{
    public class WeatherHistoryEntity: ITableEntity
    {
            public string PartitionKey { get; set; } = "";
            public string RowKey { get; set; } = "";
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            public string City { get; set; } = "";
            public decimal TemperatureC { get; set; }
            public decimal Humidity { get; set; }
            public decimal WindSpeed { get; set; }
            public string IconUrl { get; set; } = "";
            public string IconCode { get; set; } = "";
            public DateTime CreatedUtc { get; set; }
        
    }
}

