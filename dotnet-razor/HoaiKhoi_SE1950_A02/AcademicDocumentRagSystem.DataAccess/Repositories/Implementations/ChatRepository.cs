using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly AcademicRagDbContext _context;

    public ChatRepository(AcademicRagDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatSession>> GetSessionsByAccountAsync(int accountId)
    {
        return await _context.ChatSessions
            .Include(s => s.Document)
            .Include(s => s.Course)
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionByIdAsync(int chatSessionId)
    {
        return await _context.ChatSessions
            .Include(s => s.Document)
            .Include(s => s.Course)
            .Include(s => s.ChatMessages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.ChatSessionId == chatSessionId);
    }

    public async Task AddSessionAsync(ChatSession session)
    {
        await _context.ChatSessions.AddAsync(session);
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
    }

    public void UpdateSession(ChatSession session)
    {
        _context.ChatSessions.Update(session);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
