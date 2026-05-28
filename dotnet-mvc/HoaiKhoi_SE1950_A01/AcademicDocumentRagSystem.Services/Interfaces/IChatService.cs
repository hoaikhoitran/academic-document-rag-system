using AcademicDocumentRagSystem.Services.DTOs.Chat;

namespace AcademicDocumentRagSystem.Services.Interfaces;

public interface IChatService
{
    Task<List<IndexedDocumentDto>> GetIndexedDocumentsAsync();

    Task<ChatAnswerDto> AskAsync(AskQuestionDto dto, int accountId);
}