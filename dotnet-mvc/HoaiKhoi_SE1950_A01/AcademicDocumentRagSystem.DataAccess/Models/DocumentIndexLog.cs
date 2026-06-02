using System;

namespace AcademicDocumentRagSystem.DataAccess.Models;

public partial class DocumentIndexLog
{
    public int DocumentIndexLogId { get; set; }

    public int DocumentId { get; set; }

    public string Action { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? PerformedByAccountId { get; set; }

    public string PerformedByEmail { get; set; } = null!;

    public DateTime PerformedAt { get; set; }

    public int? TotalChunks { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Account? PerformedByAccount { get; set; }
}
