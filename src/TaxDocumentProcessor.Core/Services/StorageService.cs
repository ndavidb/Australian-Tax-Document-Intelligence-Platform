using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text.Json;
using TaxDocumentProcessor.Core.Models;
using ContentType = System.Net.Mime.ContentType;

namespace TaxDocumentProcessor.Core.Services;

public class StorageService(BlobServiceClient blobServiceClient) : IStorageService
{
    public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string containerName)
    {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
        {
            ContentType = GetContentType(fileName)
        });
        return blobClient.Uri.ToString();
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }

    public async Task<Stream> DownloadDocumentAsync(string blobName, string containerName)
    {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
            throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");

        var downloadInfo = await blobClient.DownloadAsync();
        return downloadInfo.Value.Content;
    }

    public async Task<bool> MoveDocumentAsync(string fileName, string sourceContainer, string targetContainer)
    {
        try
        {
            var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainer);
            var targetContainerClient = blobServiceClient.GetBlobContainerClient(targetContainer);
            
            var sourceBlobClient = sourceContainerClient.GetBlobClient(fileName);
            var targetBlobClient = targetContainerClient.GetBlobClient(fileName);

            if (!await sourceBlobClient.ExistsAsync())
                return false;

            // Start copy operation
            await targetBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
            
            // Wait for copy to complete
            await WaitForCopyToCompleteAsync(targetBlobClient);
            
            // Delete source blob after successful copy
            await sourceBlobClient.DeleteIfExistsAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while moving the document '{FileName}' from '{SourceContainer}' to '{TargetContainer}'.", fileName, sourceContainer, targetContainer);
            return false;
        }
    }

    public async Task<TaxDocument?> GetDocumentMetadataAsync(string documentId)
    {
        try
        {
            var container = blobServiceClient.GetBlobContainerClient("processed");
            var blobClient = container.GetBlobClient($"{documentId}.json");

            if (!await blobClient.ExistsAsync())
                return null;

            var downloadInfo = await blobClient.DownloadAsync();
            using var stream = downloadInfo.Value.Content;
            
            var document = await JsonSerializer.DeserializeAsync<TaxDocument>(stream);
            return document;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveDocumentMetadataAsync(TaxDocument document)
    {
        var container = blobServiceClient.GetBlobContainerClient("processed");
        var blobClient = container.GetBlobClient($"{document.Id}.json");

        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = "application/json"
        });
    }
    
    private async Task WaitForCopyToCompleteAsync(BlobClient blobClient, int maxWaitSeconds = 30)
    {
        var waitTime = TimeSpan.FromSeconds(1);
        var totalWait = TimeSpan.Zero;
        
        while (totalWait < TimeSpan.FromSeconds(maxWaitSeconds))
        {
            var properties = await blobClient.GetPropertiesAsync();
            if (properties.Value.BlobCopyStatus != CopyStatus.Pending)
                break;
            
            await Task.Delay(waitTime);
            totalWait += waitTime;
        }
    }
}