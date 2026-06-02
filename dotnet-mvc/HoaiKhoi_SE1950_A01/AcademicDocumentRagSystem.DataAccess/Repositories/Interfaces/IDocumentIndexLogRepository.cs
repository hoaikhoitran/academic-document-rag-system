using AcademicDocumentRagSystem.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces
{
    public interface IDocumentIndexLogRepository
    {
        Task<List<DocumentIndexLog>> GetByDocumentAsync(int documentId);

        Task AddAsync(DocumentIndexLog log);

        Task SaveChangesAsync();
    }
}
