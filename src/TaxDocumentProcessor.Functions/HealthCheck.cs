using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace TaxDocumentProcessor.Functions; // Fixed namespace

public class HealthCheck(ILogger<HealthCheck> logger, BlobServiceClient blobServiceClient)
{
    private static readonly string[] RequiredContainers = ["receipts", "processed", "summaries"];

    [Function(nameof(HealthCheck))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "health")]
        HttpRequest req)
    {
        try
        {
            var status = new Dictionary<string, bool>();

            foreach (var containerName in RequiredContainers)
            {
                var container = blobServiceClient.GetBlobContainerClient(containerName);
                status[containerName] = await container.ExistsAsync();
            }

            return new OkObjectResult(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                containers = status,
                version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            return new ObjectResult(new 
            { 
                status = "unhealthy", 
                error = ex.Message 
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}