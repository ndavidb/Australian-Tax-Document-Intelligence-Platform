using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    })
    .ConfigureLogging(logging => { logging.AddConsole();}).Build();

host.Run();