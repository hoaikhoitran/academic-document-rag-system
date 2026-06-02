using AcademicDocumentRagSystem.Services.DTOs.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<DocumentListItemDto>> GetByTeacherAsync(int accountId);

        Task UploadAndIndexAsync(DocumentUploadDto dto, int accountId, string email);
    }
}
