using System.Text.Json.Serialization;

namespace AcademicDocumentRagSystem.Services.DTOs.Rag;

public class RagConversationTurnDto
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;
}
