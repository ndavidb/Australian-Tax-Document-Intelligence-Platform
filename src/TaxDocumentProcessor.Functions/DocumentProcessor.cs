using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaxDocumentProcessor.Core.Models;
using TaxDocumentProcessor.Core.Services;

namespace TaxDocumentProcessor.Functions;

public class DocumentProcessor(IStorageService storageService, ILogger<DocumentProcessor> logger)
{

    [Function(nameof(DocumentProcessor))]
    public async Task Run([BlobTrigger("receipts/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
    {
        logger.LogInformation($"Processing document '{name}'.");
        var document = new TaxDocument
        {
            Id = Path.GetFileNameWithoutExtension(name),
            FileName = name,
            Status = DocumentStatus.Processing,
            UploadedAt = DateTime.UtcNow
        };

        try
        {
            await storageService.SaveDocumentMetadataAsync(document);
            logger.LogInformation($"Document '{name}' metadata saved successfully for processing stage.");

            // TODO : Add Processing with Azure Document Intelligence
            await Task.Delay(2000);
            
            await storageService.MoveDocumentAsync(name, "receipts", "processed");
            
            document.Status = DocumentStatus.Completed;
            
            document.ExtractedData = new ExpenseDetails
            {
                
                Vendor = "Example Vendor",
                Amount = 100.00m,
                Date = DateTime.UtcNow,
                Category = "Office Supplies"
            };

            await storageService.SaveDocumentMetadataAsync(document);
            
            logger.LogInformation($"Document '{name}' processed successfully.");
        }
        catch(Exception ex)
        {
            logger.LogError($"Failed to process document '{name}'. Error: {document.ProcessingError}");
            
            document.Status = DocumentStatus.Failed;
            document.ProcessingError = ex.Message;
            
            await storageService.SaveDocumentMetadataAsync(document);
            throw;
        }
    }
}