using TaxDocumentProcessor.Core.Models;

namespace TaxDocumentProcessor.Core.Services;

public interface IStorageService
{
    Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string containerName);
    Task<Stream> DownloadDocumentAsync(string blobName, string containerName);
    Task<bool> MoveDocumentAsync(string fileName, string sourceContainer, string targetContainer);
    Task<TaxDocument?> GetDocumentMetadataAsync(string documentId);
    Task SaveDocumentMetadataAsync(TaxDocument document);
}