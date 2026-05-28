// =======================================================================
// RagAskResponse.cs
// -----------------------------------------------------------------------
// Response DTO for POST /rag/ask.
// =======================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PRN222.Integration.Rag;

public class RagSource
{
    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("pageNumber")]
    public int? PageNumber { get; set; }

    [JsonPropertyName("chunkIndex")]
    public int ChunkIndex { get; set; }

    /// <summary>Plain-text preview of the chunk that supports the answer.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class RagAskResponse
{
    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public List<RagSource> Sources { get; set; } = new();
}
