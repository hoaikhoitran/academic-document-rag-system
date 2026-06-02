using System;

namespace AcademicDocumentRagSystem.Services.DTOs.Documents
{
    public class DocumentIndexLogDto
    {
        public int DocumentIndexLogId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? PerformedByAccountId { get; set; }

        public string PerformedByEmail { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }

        public int? TotalChunks { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
