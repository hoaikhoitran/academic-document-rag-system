using System;

namespace AcademicDocumentRagSystem.DataAccess.Models;

public partial class DocumentChunk
{
    public int DocumentChunkId { get; set; }

    public int DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public int? PageNumber { get; set; }

    public string ChunkText { get; set; } = null!;

    public int CharCount { get; set; }

    public int? TokenEstimate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Document Document { get; set; } = null!;
}
