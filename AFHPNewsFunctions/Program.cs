using AFHPNewsFunctions.Data;
using AFHPNewsFunctions.Models;
using AFHPNewsFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IEmailSender = AFHPNewsFunctions.Services.IEmailSender;


var builder = FunctionsApplication.CreateBuilder(args);

var connectionString = builder.Configuration["AzureDBConnection"] ?? throw new InvalidOperationException("Connection string 'AzureDBConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // configure identity settings
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ITableStorageService, TableStorageService>();

builder.Services.AddHttpClient();

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
