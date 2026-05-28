using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.DTOs.Rag;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using AcademicDocumentRagSystem.Services.RagIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IRagClient _ragClient;
        private readonly IConfiguration _configuration;

        public DocumentService(
            IDocumentRepository documentRepository,
            IRagClient ragClient,
            IConfiguration configuration)
        {
            _documentRepository = documentRepository;
            _ragClient = ragClient;
            _configuration = configuration;
        }

        public async Task UploadAndIndexAsync(DocumentUploadDto dto, int accountId, string email)
        {
            var allowedExtensions = new[] { ".pdf", ".docx", ".pptx", ".txt" };

            var extension = Path.GetExtension(dto.File.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Only PDF, DOCX, PPTX, and TXT files are allowed.");
            }

            var folder = _configuration["FileStorage:DocumentFolder"];

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new Exception("Document storage folder is not configured.");
            }

            Directory.CreateDirectory(folder);

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var document = new Document
            {
                Title = dto.Title,
                Description = dto.Description,
                CourseId = dto.CourseId,
                CourseCode = dto.CourseCode,
                Chapter = dto.Chapter,
                OriginalFileName = dto.File.FileName,
                StoredFileName = storedFileName,
                FilePath = fullPath,
                FileType = extension,
                ContentType = dto.File.ContentType,
                FileSize = dto.File.Length,
                UploadStatus = "Approved",
                IndexStatus = "Processing",
                SubmittedByAccountId = accountId,
                SubmittedByEmail = email,
                CreatedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            try
            {
                var ragResponse = await _ragClient.IndexDocumentAsync(new RagIndexRequest
                {
                    DocumentId = document.DocumentId.ToString(),
                    CourseCode = document.CourseCode,
                    Chapter = document.Chapter,
                    FilePath = document.FilePath,
                    FileName = document.OriginalFileName
                });

                document.IndexStatus = "Indexed";
                document.TotalChunks = ragResponse.TotalChunks;
                document.IndexedAt = DateTime.UtcNow;
                document.IndexError = null;
            }
            catch (Exception ex)
            {
                document.IndexStatus = "Failed";
                document.IndexError = ex.Message;
            }

            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync();
        }
    }
}
