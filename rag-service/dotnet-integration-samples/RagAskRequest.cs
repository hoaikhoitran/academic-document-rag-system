// =======================================================================
// RagAskRequest.cs
// -----------------------------------------------------------------------
// Request DTO for POST /rag/ask.
// =======================================================================
using System.Text.Json.Serialization;

namespace PRN222.Integration.Rag;

public class RagAskRequest
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    /// <summary>Number of chunks to retrieve. Leave null to use the server default.</summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}
