using System.Text.Json;
using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.DTOs.Rag;
using AcademicDocumentRagSystem.Services.Interfaces;
using AcademicDocumentRagSystem.Services.RagIntegration;

namespace AcademicDocumentRagSystem.Services.Implementations;

public class ChatService : IChatService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IRagClient _ragClient;

    public ChatService(
        IDocumentRepository documentRepository,
        IChatRepository chatRepository,
        IRagClient ragClient)
    {
        _documentRepository = documentRepository;
        _chatRepository = chatRepository;
        _ragClient = ragClient;
    }

    public async Task<List<IndexedDocumentDto>> GetIndexedDocumentsAsync()
    {
        var documents = await _documentRepository.GetIndexedDocumentsAsync();

        return documents.Select(d => new IndexedDocumentDto
        {
            DocumentId = d.DocumentId,
            Title = d.Title,
            CourseCode = d.CourseCode,
            OriginalFileName = d.OriginalFileName,
            Chapter = d.Chapter
        }).ToList();
    }

    public async Task<List<ChatSessionDto>> GetSessionsAsync(int accountId)
    {
        var sessions = await _chatRepository.GetSessionsByAccountAsync(accountId);

        return sessions.Select(s => new ChatSessionDto
        {
            ChatSessionId = s.ChatSessionId,
            DocumentId = s.DocumentId,
            Title = s.Title ?? $"Session #{s.ChatSessionId}",
            DocumentTitle = s.Document.Title,
            CourseCode = s.Course.Code,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }

    public async Task<ChatSessionDetailsDto?> GetSessionAsync(int chatSessionId, int accountId)
    {
        var session = await _chatRepository.GetSessionByIdAsync(chatSessionId);

        if (session == null || session.AccountId != accountId)
        {
            return null;
        }

        return new ChatSessionDetailsDto
        {
            ChatSessionId = session.ChatSessionId,
            DocumentId = session.DocumentId,
            Title = session.Title ?? $"Session #{session.ChatSessionId}",
            DocumentTitle = session.Document.Title,
            CourseCode = session.Course.Code,
            Messages = session.ChatMessages
                .OrderBy(m => m.CreatedAt)
                .Select(MapMessage)
                .ToList()
        };
    }

    public async Task<ChatAnswerDto> AskAsync(AskQuestionDto dto, int accountId)
    {
        var document = await _documentRepository.GetByIdAsync(dto.DocumentId);

        if (document == null)
        {
            throw new Exception("Document not found.");
        }

        if (document.IndexStatus != "Indexed")
        {
            throw new Exception("This document has not been indexed yet.");
        }

        var session = await GetOrCreateSessionAsync(dto, accountId, document);
        var history = BuildConversationHistory(session);

        var ragResponse = await _ragClient.AskAsync(new RagAskRequest
        {
            SessionId = session.ChatSessionId.ToString(),
            UserId = accountId.ToString(),
            CourseCode = document.CourseCode,
            DocumentId = document.DocumentId.ToString(),
            Question = dto.Question,
            TopK = 5,
            ConversationHistory = history
        });

        var sourcesJson = JsonSerializer.Serialize(ragResponse.Sources);

        var message = new ChatMessage
        {
            ChatSessionId = session.ChatSessionId,
            AccountId = accountId,
            DocumentId = document.DocumentId,
            Question = dto.Question,
            Answer = ragResponse.Answer,
            SourcesJson = sourcesJson,
            CreatedAt = DateTime.UtcNow
        };

        await _chatRepository.AddMessageAsync(message);

        session.UpdatedAt = DateTime.UtcNow;
        _chatRepository.UpdateSession(session);

        await _chatRepository.SaveChangesAsync();

        return new ChatAnswerDto
        {
            ChatSessionId = session.ChatSessionId,
            DocumentId = document.DocumentId,
            Question = dto.Question,
            Answer = ragResponse.Answer,
            Sources = ragResponse.Sources
        };
    }

    private async Task<ChatSession> GetOrCreateSessionAsync(AskQuestionDto dto, int accountId, Document document)
    {
        if (dto.ChatSessionId.HasValue)
        {
            var existingSession = await _chatRepository.GetSessionByIdAsync(dto.ChatSessionId.Value);

            if (existingSession == null ||
                existingSession.AccountId != accountId ||
                existingSession.DocumentId != document.DocumentId)
            {
                throw new Exception("Chat session not found.");
            }

            return existingSession;
        }

        var session = new ChatSession
        {
            AccountId = accountId,
            CourseId = document.CourseId,
            DocumentId = document.DocumentId,
            Title = dto.Question.Length > 80 ? dto.Question.Substring(0, 80) : dto.Question,
            CreatedAt = DateTime.UtcNow
        };

        await _chatRepository.AddSessionAsync(session);
        await _chatRepository.SaveChangesAsync();

        return session;
    }

    public async Task<ChatWorkspaceDto> GetWorkspaceAsync(int accountId, int? documentId, int? sessionId)
    {
        var documents = await GetIndexedDocumentsAsync();
        var sessions = await GetSessionsAsync(accountId);

        var workspace = new ChatWorkspaceDto
        {
            Documents = documents,
            Sessions = sessions,
            SelectedDocumentId = documentId,
            SelectedSessionId = sessionId
        };

        if (documentId == null && sessions.Count > 0 && sessionId == null)
        {
            var latest = sessions.OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt).First();
            sessionId = latest.ChatSessionId;
            documentId = latest.DocumentId;
            workspace.SelectedSessionId = sessionId;
            workspace.SelectedDocumentId = documentId;
        }

        if (documentId.HasValue)
        {
            workspace.ActiveDocument = documents.FirstOrDefault(d => d.DocumentId == documentId.Value);
            workspace.AskForm.DocumentId = documentId.Value;
            workspace.AskForm.ChatSessionId = sessionId;
        }

        if (sessionId.HasValue)
        {
            workspace.ActiveSession = await GetSessionAsync(sessionId.Value, accountId);
            if (workspace.ActiveSession != null)
            {
                workspace.AskForm.DocumentId = workspace.ActiveSession.DocumentId;
                workspace.AskForm.ChatSessionId = workspace.ActiveSession.ChatSessionId;
                workspace.ActiveDocument = documents.FirstOrDefault(d =>
                    d.DocumentId == workspace.ActiveSession.DocumentId);
            }
        }
        else if (documentId.HasValue && sessionId == null)
        {
            workspace.ShowDocumentPicker = workspace.ActiveSession == null;
        }

        return workspace;
    }

    private static ChatMessageDto MapMessage(ChatMessage m)
    {
        var sources = new List<RagSourceDto>();
        if (!string.IsNullOrWhiteSpace(m.SourcesJson))
        {
            try
            {
                sources = JsonSerializer.Deserialize<List<RagSourceDto>>(m.SourcesJson) ?? new();
            }
            catch
            {
                sources = new List<RagSourceDto>();
            }
        }

        return new ChatMessageDto
        {
            ChatMessageId = m.ChatMessageId,
            Question = m.Question,
            Answer = m.Answer,
            CreatedAt = m.CreatedAt,
            Sources = sources
        };
    }

    private static List<RagConversationTurnDto> BuildConversationHistory(ChatSession session)
    {
        return session.ChatMessages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(6)
            .Select(m => new RagConversationTurnDto
            {
                Question = m.Question,
                Answer = m.Answer
            })
            .ToList();
    }
}
