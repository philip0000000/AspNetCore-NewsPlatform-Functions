using ElPriceAPIDataAutoSave.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();//Critical for HttpClient injection
        services.AddHttpClient<ElPriceService>();
        services.AddSingleton<IElPriceHistoryService, ElPriceHistoryService>();
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();