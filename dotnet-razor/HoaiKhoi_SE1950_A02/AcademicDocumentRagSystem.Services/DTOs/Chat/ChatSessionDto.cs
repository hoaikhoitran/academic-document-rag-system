namespace AcademicDocumentRagSystem.Services.DTOs.Chat;

public class ChatSessionDto
{
    public int ChatSessionId { get; set; }

    public int DocumentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public string CourseCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
