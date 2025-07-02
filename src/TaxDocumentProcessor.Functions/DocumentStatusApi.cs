using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxDocumentProcessor.Core.Services;

namespace TaxDocumentProcessor.Functions;

public class DocumentStatusApi(ILogger<DocumentStatusApi> logger, IStorageService storageService)
{
    [Function("GetStatus")]
    public async Task<IActionResult> GetStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/{documentId}/status")] HttpRequest req, string documentId)
    {
        try
        {
            var document = await storageService.GetDocumentMetadataAsync(documentId);

            if (document == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Document not found",
                    documentId,
                    timestamp = DateTime.UtcNow
                });
            }

            return new OkObjectResult(new
            {
                id = document.Id,
                filename = document.FileName,
                status = document.Status,
                uploadedAt = document.UploadedAt,
                extractedData = document.ExtractedData,
                processingError = document.ProcessingError,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error retrieving status for document {documentId}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            
        }
    }

}