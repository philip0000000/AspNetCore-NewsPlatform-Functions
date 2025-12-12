using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherAPIDataAutoSave.Models
{
    public class WeatherDto
    {
        public string City { get; set; } = string.Empty;
        public decimal TemperatureC { get; set; }
        public decimal Humidity { get; set; }
        public decimal WindSpeed { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
