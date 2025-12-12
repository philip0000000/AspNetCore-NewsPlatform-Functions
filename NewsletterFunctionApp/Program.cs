//using Azure.Data.Tables;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Builder;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using NewsletterFunctionApp.Services.Newsletter;

//var builder = FunctionsApplication.CreateBuilder(args);

//builder.ConfigureFunctionsWebApplication();

//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();

//builder.Build().Run();

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AFHPNewsFunctions.Services;
using NewsletterFunctionApp.Services.Newsletter;
using Azure.Data.Tables;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults() // ? should compile now
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // Table client
        var storageConn = cfg["AzureWebJobsStorage"];
        services.AddSingleton<TableServiceClient>(_ => new TableServiceClient(storageConn));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<TableServiceClient>()
                           .GetTableClient("NewsletterSubscriptions");
            client.CreateIfNotExists();
            return client;
        });

        //// Options
        //services.AddOptions<NewsletterOptions>()
        //        .Bind(cfg.GetSection("Newsletter"))
        //        .ValidateDataAnnotations();

        // Services
        services.AddSingleton<IEmailSender, EmailSender>();
        //services.AddSingleton<INewsletterTemplateService, NewsletterTemplateService>();
        services.AddSingleton<INewsletterService, AzureNewsletterService>();
    })
    .Build();

await host.RunAsync();

public class NewsletterOptions
{
    public bool DryRun { get; set; } = true;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}
