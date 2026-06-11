namespace AcademicDocumentRagSystem.Services.DTOs.Chat;

public class ChatSessionDetailsDto
{
    public int ChatSessionId { get; set; }

    public int DocumentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public string CourseCode { get; set; } = string.Empty;

    public List<ChatMessageDto> Messages { get; set; } = new();
}
