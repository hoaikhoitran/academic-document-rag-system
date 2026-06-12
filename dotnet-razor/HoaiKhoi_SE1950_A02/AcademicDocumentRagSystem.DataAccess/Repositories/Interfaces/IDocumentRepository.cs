using AcademicDocumentRagSystem.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetAllAsync();

        Task<List<Document>> GetBySubmitterAsync(int accountId);

        /// <summary>
        /// All documents (optionally filtered) for the admin document list, including
        /// course and uploader navigation properties.
        /// </summary>
        Task<List<Document>> GetForAdminAsync(int? courseId, string? courseCode, string? uploadStatus, string? indexStatus);

        /// <summary>
        /// Returns the existing active (non-deleted) document with the same content hash
        /// in the given course, or null when the file content has not been uploaded yet.
        /// </summary>
        Task<Document?> GetActiveByCourseAndFileHashAsync(int courseId, string fileHashSha256);

        Task<Document?> GetByIdAsync(int id);

        Task AddAsync(Document document);

        void Update(Document document);
        Task<List<Document>> GetIndexedDocumentsAsync();
        Task SaveChangesAsync();
    }
}
