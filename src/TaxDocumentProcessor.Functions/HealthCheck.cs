using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Net;

namespace TaxDocumentProcessor.Functions;

public class HealthCheck(ILogger<HealthCheck> logger, BlobServiceClient blobServiceClient)
{
    private static readonly string[] RequiredContainers = ["receipts", "processed", "summaries"];

    [Function(nameof(HealthCheck))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "health")]
        HttpRequestData req)
    {
        try
        {
            var status = new Dictionary<string, bool>();

            foreach (var containerName in RequiredContainers)
            {
                var container = blobServiceClient.GetBlobContainerClient(containerName);
                status[containerName] = await container.ExistsAsync();
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                containers = status,
                version = "1.0.0"
            });
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new 
            { 
                status = "unhealthy", 
                error = ex.Message 
            });
            return response;
        }
    }
}