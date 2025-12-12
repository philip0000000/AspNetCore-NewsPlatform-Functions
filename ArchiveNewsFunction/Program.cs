using ArchiveNewsFunction.Data;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["SqlConnectionString"];

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
    })
    .Build();

host.Run();
