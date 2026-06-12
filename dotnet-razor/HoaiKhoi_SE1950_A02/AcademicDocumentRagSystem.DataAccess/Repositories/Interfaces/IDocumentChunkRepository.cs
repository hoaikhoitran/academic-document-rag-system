using AcademicDocumentRagSystem.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces
{
    public interface IDocumentChunkRepository
    {
        Task<List<DocumentChunk>> GetByDocumentAsync(int documentId);

        Task<int> CountByDocumentAsync(int documentId);

        Task<List<int>> GetDocumentIdsWithChunksAsync(List<int> documentIds);

        Task AddRangeAsync(IEnumerable<DocumentChunk> chunks);

        Task DeleteByDocumentAsync(int documentId);

        Task SaveChangesAsync();
    }
}
