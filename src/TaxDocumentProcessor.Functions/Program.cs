using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaxDocumentProcessor.Core.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(
                Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        });
        
        services.AddScoped<IStorageService, StorageService>();

    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.AddApplicationInsights();
    }).Build();

host.Run();