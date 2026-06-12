using AcademicDocumentRagSystem.DataAccess.Models;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;

public interface IChatRepository
{
    Task<List<ChatSession>> GetSessionsByAccountAsync(int accountId);

    Task<ChatSession?> GetSessionByIdAsync(int chatSessionId);

    Task AddSessionAsync(ChatSession session);

    Task AddMessageAsync(ChatMessage message);

    void UpdateSession(ChatSession session);

    Task SaveChangesAsync();
}
