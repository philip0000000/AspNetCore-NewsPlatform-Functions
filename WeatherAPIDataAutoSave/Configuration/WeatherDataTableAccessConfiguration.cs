using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherAPIDataAutoSave.Configuration
{
    internal class WeatherDataTableAccessConfiguration
    {
        public string AzureWebJobsStorage { get; set; } = string.Empty;
    }
}
