//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//var builder = FunctionsApplication.CreateBuilder(args);

//builder.ConfigureFunctionsWebApplication();

//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();

//builder.Build().Run();

using AFHP_NewsSite.Services;
using AFHP_NewsSite.Services.Claims_Roles;
using AFHP_NewsSite.Services.Newsletter;
using AFHPNewsFunctions.Data;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NewsletterBodyFunction.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// --- Configuration ---
builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// --- Isolated worker features ---
builder.ConfigureFunctionsWebApplication();

// --- Application Insights ---
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// --- Table Storage ---
var storageConn = builder.Configuration["AzureWebJobsStorage"];
builder.Services.AddSingleton(new TableServiceClient(storageConn));
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<TableServiceClient>()
                   .GetTableClient("NewsletterSubscriptions");
    client.CreateIfNotExists();
    return client;
});

// --- SQL DbContext (for articles) ---
var sqlConn = builder.Configuration.GetConnectionString("NewsletterDb");
if (!string.IsNullOrEmpty(sqlConn))
{
    builder.Services.AddDbContext<NewsletterDbContext>(options =>
        options.UseSqlServer(sqlConn));
}

// --- NewsletterOptions binding ---
builder.Services.Configure<NewsletterOptions>(
    builder.Configuration.GetSection("Email"));

// --- Core Services ---
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<INewsletterService, AzureNewsletterService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<INewsletterTemplateService, NewsletterTemplateService>();


// --- Startup validation ---
using (var tempProvider = builder.Services.BuildServiceProvider())
{
    var options = tempProvider.GetRequiredService<IOptions<NewsletterOptions>>().Value;

    Console.WriteLine("=== NewsletterOptions Startup Check ===");
    Console.WriteLine($"DryRun: {options.DryRun}");
    Console.WriteLine($"FromEmail: {options.FromEmail}");
    Console.WriteLine($"FromName: {options.FromName}");
    Console.WriteLine($"SmtpServer: {options.SmtpServer}");
    Console.WriteLine($"SmtpPort: {options.SmtpPort}");
    Console.WriteLine($"SmtpUsername: {options.SmtpUsername}");
    Console.WriteLine("SmtpPassword: ******** (masked)");
    Console.WriteLine("======================================");

    if (string.IsNullOrWhiteSpace(options.FromEmail) ||
        string.IsNullOrWhiteSpace(options.FromName) ||
        string.IsNullOrWhiteSpace(options.SmtpServer) ||
        options.SmtpPort <= 0 ||
        string.IsNullOrWhiteSpace(options.SmtpUsername) ||
        string.IsNullOrWhiteSpace(options.SmtpPassword))
    {
        throw new InvalidOperationException("NewsletterOptions is missing required SMTP configuration.");
    }
}

builder.Build().Run();

// --- Strongly typed options for newsletter config ---
public class NewsletterOptions
{
    public bool DryRun { get; set; } = true;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string? FromEmail { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string? FromName { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string? SmtpServer { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public int SmtpPort { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string? SmtpUsername { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string? SmtpPassword { get; set; }
}

