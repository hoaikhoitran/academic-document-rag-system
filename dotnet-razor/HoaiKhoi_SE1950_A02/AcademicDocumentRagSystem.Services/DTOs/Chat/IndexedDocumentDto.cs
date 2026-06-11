namespace AcademicDocumentRagSystem.Services.DTOs.Chat;

public class IndexedDocumentDto
{
    public int DocumentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CourseCode { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string? Chapter { get; set; }
}