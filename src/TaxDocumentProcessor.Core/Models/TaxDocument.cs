namespace TaxDocumentProcessor.Core.Models;

public class TaxDocument
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public ExpenseDetails? ExpenseDetails { get; set; }
    public string? ProcessingError { get; set; } = null;
}

public enum DocumentStatus
{
    Pending, 
    Processing,
    Completed,
    Failed
}

public class ExpenseDetails
{
    public string Vendor { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<LineItem> LineItems { get; set; } = [];
}

public class LineItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
