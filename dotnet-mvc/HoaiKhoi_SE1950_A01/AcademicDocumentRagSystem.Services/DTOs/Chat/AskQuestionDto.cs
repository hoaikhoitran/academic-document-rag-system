using System.ComponentModel.DataAnnotations;

namespace AcademicDocumentRagSystem.Services.DTOs.Chat;

public class AskQuestionDto
{
    [Required]
    public int DocumentId { get; set; }

    [Required]
    public string Question { get; set; } = string.Empty;
}