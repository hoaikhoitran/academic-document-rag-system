// =======================================================================
// RagIndexResponse.cs
// -----------------------------------------------------------------------
// Response DTO for POST /rag/index-document.
// =======================================================================
using System.Text.Json.Serialization;

namespace PRN222.Integration.Rag;

public class RagIndexResponse
{
    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>Always "indexed" on success.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("totalChunks")]
    public int TotalChunks { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
