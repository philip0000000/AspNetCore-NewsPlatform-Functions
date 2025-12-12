using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherAPIDataAutoSave.Models;
using WeatherAPIDataAutoSave.Tables;

namespace WeatherAPIDataAutoSave.Services
{
    public interface IWeatherHistoryService
    {
        //Task EnsureTableAsync();
        Task AddSnapshotAsync(string city, WeatherForecastVM forecast);


        Task<IReadOnlyList<WeatherHistoryEntity>> GetHistoryAsync(string city, DateTime from, DateTime to);
    }
}
