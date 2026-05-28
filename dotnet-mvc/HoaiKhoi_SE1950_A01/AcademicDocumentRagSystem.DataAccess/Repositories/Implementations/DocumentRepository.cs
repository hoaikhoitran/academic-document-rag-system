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

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Course)
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
