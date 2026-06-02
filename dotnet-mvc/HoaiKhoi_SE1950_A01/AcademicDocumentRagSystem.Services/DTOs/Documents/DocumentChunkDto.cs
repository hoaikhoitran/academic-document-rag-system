namespace AcademicDocumentRagSystem.Services.DTOs.Documents
{
    public class DocumentChunkDto
    {
        public int ChunkIndex { get; set; }

        public int? PageNumber { get; set; }

        public string ChunkText { get; set; } = string.Empty;

        public int CharCount { get; set; }

        public int? TokenEstimate { get; set; }
    }
}
