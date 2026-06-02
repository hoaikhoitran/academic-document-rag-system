using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<DocumentListItemDto>> GetByTeacherAsync(int accountId);

        /// <summary>
        /// Validates teacher/course permission, saves the file, generates MVC-side
        /// chunk preview rows, calls the RAG index service and writes audit logs.
        /// Returns the new document id so the caller can redirect to the preview page.
        /// </summary>
        Task<int> UploadAndIndexAsync(DocumentUploadDto dto, int accountId, string email);

        /// <summary>Courses a teacher may upload to (their assigned course only).</summary>
        Task<List<CourseDto>> GetUploadCoursesForTeacherAsync(int accountId);

        /// <summary>
        /// Document metadata + chunk preview + audit logs. Returns null when the
        /// viewer is not allowed to see the document.
        /// </summary>
        Task<DocumentDetailsDto?> GetDetailsAsync(int documentId, int? accountId, string roleName);

        /// <summary>
        /// Rebuilds the chunk preview and re-runs RAG indexing without duplicating
        /// chunk rows. Throws when the viewer is not allowed to re-index.
        /// </summary>
        Task ReIndexAsync(int documentId, int? accountId, string email, string roleName);
    }
}
