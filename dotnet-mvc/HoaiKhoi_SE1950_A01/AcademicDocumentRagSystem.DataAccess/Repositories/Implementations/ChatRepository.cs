using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly AcademicRagDbContext _context;

    public ChatRepository(AcademicRagDbContext context)
    {
        _context = context;
    }

    public async Task AddSessionAsync(ChatSession session)
    {
        await _context.ChatSessions.AddAsync(session);
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}