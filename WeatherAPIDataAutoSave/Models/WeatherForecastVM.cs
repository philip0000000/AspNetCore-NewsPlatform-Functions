using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherAPIDataAutoSave.Models
{
    public class WeatherForecastVM
    {
        public string Summary { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;
        public decimal TemperatureC { get; set; }
        public decimal TemperatureF { get; set; }
        public decimal Humidity { get; set; }
        public decimal WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public long UnixTime { get; set; }
        public IconData Icon { get; set; } = new IconData();
        public string Code { get; set; } = string.Empty;
    }

    public class IconData
    {
        public string Url { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
