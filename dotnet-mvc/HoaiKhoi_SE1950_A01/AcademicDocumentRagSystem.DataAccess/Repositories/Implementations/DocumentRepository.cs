using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AcademicRagDbContext _context;

        public DocumentRepository(AcademicRagDbContext context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(d => d.Course)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetBySubmitterAsync(int accountId)
        {
            return await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.SubmittedByAccount)
                .Where(d => d.SubmittedByAccountId == accountId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetForAdminAsync(
            int? courseId, string? courseCode, string? uploadStatus, string? indexStatus)
        {
            var query = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.SubmittedByAccount)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(d => d.CourseId == courseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(courseCode))
            {
                query = query.Where(d => d.CourseCode == courseCode);
            }

            if (!string.IsNullOrWhiteSpace(uploadStatus))
            {
                query = query.Where(d => d.UploadStatus == uploadStatus);
            }

            if (!string.IsNullOrWhiteSpace(indexStatus))
            {
                query = query.Where(d => d.IndexStatus == indexStatus);
            }

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Document?> GetActiveByCourseAndFileHashAsync(int courseId, string fileHashSha256)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d =>
                    d.CourseId == courseId &&
                    d.FileHashSha256 == fileHashSha256 &&
                    d.UploadStatus != "Deleted");
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.SubmittedByAccount)
                .FirstOrDefaultAsync(d => d.DocumentId == id);
        }

        public async Task AddAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
        }

        public void Update(Document document)
        {
            _context.Documents.Update(document);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<Document>> GetIndexedDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.Course)
                .Where(d => d.IndexStatus == "Indexed" && d.UploadStatus != "Deleted")
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}
