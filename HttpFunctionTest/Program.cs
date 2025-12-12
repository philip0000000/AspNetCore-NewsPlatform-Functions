using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .Configure<LoggerFilterOptions>(options =>
    {
        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs.
        // Application Insights requires an explicit override.
        // For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule =>
            rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

        if (toRemove is not null)
            options.Rules.Remove(toRemove);
    });

    builder.Build().Run();
