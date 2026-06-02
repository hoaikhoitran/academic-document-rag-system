namespace AcademicDocumentRagSystem.Services.Chunking
{
    /// <summary>
    /// A single MVC-side chunk preview (no embeddings, no vector data).
    /// </summary>
    public class ChunkPreviewItem
    {
        public int ChunkIndex { get; set; }

        public int? PageNumber { get; set; }

        public string ChunkText { get; set; } = string.Empty;

        public int CharCount { get; set; }

        public int? TokenEstimate { get; set; }
    }
}
