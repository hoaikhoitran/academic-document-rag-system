using AcademicDocumentRagSystem.DataAccess.Models;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;

public interface IChatRepository
{
    Task AddSessionAsync(ChatSession session);

    Task AddMessageAsync(ChatMessage message);

    Task SaveChangesAsync();
}