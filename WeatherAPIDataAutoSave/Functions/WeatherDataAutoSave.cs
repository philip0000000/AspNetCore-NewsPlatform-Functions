using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WeatherAPIDataAutoSave.Models;
using WeatherAPIDataAutoSave.Services;

namespace WeatherAPIDataAutoSave.Functions
{
    public class WeatherDataAutoSave
    {
        private readonly ILogger<WeatherDataAutoSave> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWeatherHistoryService _historyService;

        public WeatherDataAutoSave(
            ILogger<WeatherDataAutoSave> logger,
            HttpClient httpClient,
            IWeatherHistoryService historyService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _historyService = historyService;
        }

        [Function("WeatherDataAutoSave")]
        public async Task Run([TimerTrigger("0 0 13 * * *")] TimerInfo myTimer) // 13 :00 UTC daily
        {
            _logger.LogInformation($"WeatherDataAutoSave started at {DateTime.UtcNow}");

            var cities = new[] { "Stockholm", "New York", "Tokyo", "Sydney", "Cape Town" };

            foreach (var city in cities)
            {
                try
                {
                    string url = $"https://weatherapi.dreammaker-it.se/Forecast?city={city}&lang=en";
                    _logger.LogInformation($"Fetching weather for {city}");

                    var response = await _httpClient.GetAsync(url);
                    _logger.LogInformation($"Weather API status for {city}: {response.StatusCode}");

                    response.EnsureSuccessStatusCode();

                    var forecast = await response.Content.ReadFromJsonAsync<WeatherForecastVM>();

                    if (forecast == null)
                    {
                        _logger.LogWarning($"No weather data returned for {city}");
                        continue;
                    }

                    // Save snapshot via your service
                    await _historyService.AddSnapshotAsync(city, forecast);

                    _logger.LogInformation($"Snapshot saved for {city} at {DateTime.UtcNow}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing city: {city}");
                }
            }
        }
    }
}
