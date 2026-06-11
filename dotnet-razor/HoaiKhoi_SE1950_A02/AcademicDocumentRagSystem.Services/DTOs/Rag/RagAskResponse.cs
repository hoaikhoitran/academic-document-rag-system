using System.Text.Json.Serialization;

namespace AcademicDocumentRagSystem.Services.DTOs.Rag
{
    public class RagAskResponse
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("sources")]
        public List<RagSourceDto> Sources { get; set; } = new();
    }
}