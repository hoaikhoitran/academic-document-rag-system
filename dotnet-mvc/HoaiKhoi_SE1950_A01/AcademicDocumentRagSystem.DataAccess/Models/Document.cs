using System;
using System.Collections.Generic;

namespace AcademicDocumentRagSystem.DataAccess.Models;

public partial class Document
{
    public int DocumentId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CourseId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string? Chapter { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public string UploadStatus { get; set; } = null!;

    public string IndexStatus { get; set; } = null!;

    public int TotalChunks { get; set; }

    public string? IndexError { get; set; }

    public int? SubmittedByAccountId { get; set; }

    public string? SubmittedByEmail { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? IndexedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    public virtual Course Course { get; set; } = null!;

    public virtual Account? SubmittedByAccount { get; set; }
}
