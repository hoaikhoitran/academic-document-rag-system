namespace AcademicDocumentRagSystem.Services.DTOs.Documents;

/// <summary>
/// Optional filters for the admin document list. Any null/empty value is ignored.
/// </summary>
public class DocumentFilterDto
{
    public int? CourseId { get; set; }

    public string? CourseCode { get; set; }

    public string? UploadStatus { get; set; }

    public string? IndexStatus { get; set; }
}
