using System;
using System.Collections.Generic;

namespace AcademicDocumentRagSystem.DataAccess.Models;

public partial class ChatSession
{
    public int ChatSessionId { get; set; }

    public int AccountId { get; set; }

    public int CourseId { get; set; }

    public int DocumentId { get; set; }

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual Course Course { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
