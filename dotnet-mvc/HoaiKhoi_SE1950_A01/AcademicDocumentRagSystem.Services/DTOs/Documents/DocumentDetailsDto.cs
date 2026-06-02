using System;
using System.Collections.Generic;

namespace AcademicDocumentRagSystem.Services.DTOs.Documents
{
    public class DocumentDetailsDto
    {
        public int DocumentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string? CourseName { get; set; }

        public string? Chapter { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string UploadStatus { get; set; } = string.Empty;

        public string IndexStatus { get; set; } = string.Empty;

        public int TotalChunks { get; set; }

        public string? IndexError { get; set; }

        public string? SubmittedByEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? IndexedAt { get; set; }

        /// <summary>
        /// Message shown when no preview chunks could be extracted
        /// (e.g. scanned / image-only PDF).
        /// </summary>
        public string? PreviewMessage { get; set; }

        public List<DocumentChunkDto> Chunks { get; set; } = new();

        public List<DocumentIndexLogDto> IndexLogs { get; set; } = new();

        /// <summary>True when the current viewer may trigger a re-index.</summary>
        public bool CanReIndex { get; set; }
    }
}
