using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherAPIDataAutoSave.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();//Critical for HttpClient injection
        services.AddSingleton<IWeatherHistoryService, WeatherHistoryService>();

       
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
