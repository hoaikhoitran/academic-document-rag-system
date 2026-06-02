using System.Collections.Generic;

namespace AcademicDocumentRagSystem.Services.Chunking
{
    /// <summary>
    /// Result of MVC-side chunk preview generation. When <see cref="Success"/> is
    /// false the upload/indexing flow must continue, but <see cref="ErrorMessage"/>
    /// explains why no preview chunks were produced.
    /// </summary>
    public class ChunkPreviewResult
    {
        public bool Success { get; set; }

        public List<ChunkPreviewItem> Items { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public static ChunkPreviewResult Ok(List<ChunkPreviewItem> items) =>
            new() { Success = true, Items = items };

        public static ChunkPreviewResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
