using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Implementations
{
    public class DocumentChunkRepository : IDocumentChunkRepository
    {
        private readonly AcademicRagDbContext _context;

        public DocumentChunkRepository(AcademicRagDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentChunk>> GetByDocumentAsync(int documentId)
        {
            return await _context.DocumentChunks
                .Where(c => c.DocumentId == documentId)
                .OrderBy(c => c.ChunkIndex)
                .ToListAsync();
        }

        public async Task<int> CountByDocumentAsync(int documentId)
        {
            return await _context.DocumentChunks
                .CountAsync(c => c.DocumentId == documentId);
        }

        public async Task<List<int>> GetDocumentIdsWithChunksAsync(List<int> documentIds)
        {
            if (documentIds == null || documentIds.Count == 0)
            {
                return new List<int>();
            }

            return await _context.DocumentChunks
                .Where(c => documentIds.Contains(c.DocumentId))
                .Select(c => c.DocumentId)
                .Distinct()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks)
        {
            await _context.DocumentChunks.AddRangeAsync(chunks);
        }

        public async Task DeleteByDocumentAsync(int documentId)
        {
            var existing = await _context.DocumentChunks
                .Where(c => c.DocumentId == documentId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                _context.DocumentChunks.RemoveRange(existing);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
