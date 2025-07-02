using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxDocumentProcessor.Core.Services;

namespace TaxDocumentProcessor.Functions;

public class DocumentUploadApi(ILogger<DocumentUploadApi> logger, IStorageService storageService)
{
    [Function("UploadDocument")]
    public async Task<IActionResult> UploadDocument([HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents/upload")] HttpRequest req)
    {
        try
        {
            if (!req.Form.Files.Any())
            {
                return new BadRequestObjectResult("No files uploaded.");
            }
            
            var file = req.Form.Files[0];
            
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".json" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return new BadRequestObjectResult($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
            }
            
            if(file.Length > 10 * 1024 * 1024) // 10 MB limit
            {
                return new BadRequestObjectResult("File size exceeds the 10 MB limit.");
            }
            
            var filename = $"{Guid.NewGuid()}{fileExtension}";
            
            using var fileStream = file.OpenReadStream();
            var blobUrl = await storageService.UploadDocumentAsync(fileStream, filename, "receipts");
            
            logger.LogInformation($"File uploaded successfully: {blobUrl}");
            
            return new OkObjectResult(new
            {
                message = "File uploaded successfully",
                documentId = Path.GetFileNameWithoutExtension(filename),
                fileUrl = blobUrl,
                fileName = filename,
                originalName = file.FileName,
                timestamp = DateTime.UtcNow
            });
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error uploading document");
            return new ObjectResult(new
            {
                message = "Error uploading document",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

}