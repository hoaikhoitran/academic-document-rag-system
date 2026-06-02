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

        Task<Document?> GetByIdAsync(int id);

        Task AddAsync(Document document);

        void Update(Document document);
        Task<List<Document>> GetIndexedDocumentsAsync();
        Task SaveChangesAsync();
    }
}
