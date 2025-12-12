using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(_ =>
            new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage")));
    })
    .Build();

host.Run();
