using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Implementations
{
    public class DocumentIndexLogRepository : IDocumentIndexLogRepository
    {
        private readonly AcademicRagDbContext _context;

        public DocumentIndexLogRepository(AcademicRagDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentIndexLog>> GetByDocumentAsync(int documentId)
        {
            return await _context.DocumentIndexLogs
                .Where(l => l.DocumentId == documentId)
                .OrderByDescending(l => l.PerformedAt)
                .ToListAsync();
        }

        public async Task AddAsync(DocumentIndexLog log)
        {
            await _context.DocumentIndexLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
