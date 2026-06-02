namespace AcademicDocumentRagSystem.Services.DTOs.Documents;

public class DocumentListItemDto
{
    public int DocumentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string? Chapter { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string UploadStatus { get; set; } = string.Empty;

    public string IndexStatus { get; set; } = string.Empty;

    public int TotalChunks { get; set; }

    public string? IndexError { get; set; }

    public string? SubmittedByEmail { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? IndexedAt { get; set; }

    /// <summary>True when at least one chunk preview row is stored for this document.</summary>
    public bool HasChunks { get; set; }
}
