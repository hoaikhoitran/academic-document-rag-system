using System;
using System.Collections.Generic;

namespace AcademicDocumentRagSystem.DataAccess.Models;

public partial class ChatMessage
{
    public int ChatMessageId { get; set; }

    public int ChatSessionId { get; set; }

    public int AccountId { get; set; }

    public int DocumentId { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public string SourcesJson { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ChatSession ChatSession { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
