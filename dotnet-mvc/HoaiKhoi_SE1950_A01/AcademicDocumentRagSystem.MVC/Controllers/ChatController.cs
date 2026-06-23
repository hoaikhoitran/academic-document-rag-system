using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

/// <summary>Legacy routes — redirect to EduRAG Student/Teacher chat workspace.</summary>
[SessionAuthorize("Student", "Teacher")]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    public IActionResult Index()
    {
        return RedirectToRoleChat();
    }

    public IActionResult Sessions()
    {
        return RedirectToRoleChat();
    }

    public IActionResult Session(int id)
    {
        var role = HttpContext.Session.GetString("RoleName");
        if (role == "Teacher")
        {
            return RedirectToAction("Chat", "Teacher", new { sessionId = id });
        }

        return RedirectToAction("Chat", "Student", new { sessionId = id });
    }

    public IActionResult Ask(int documentId, int? chatSessionId)
    {
        var role = HttpContext.Session.GetString("RoleName");
        if (role == "Teacher")
        {
            return RedirectToAction("Chat", "Teacher", new { documentId, sessionId = chatSessionId });
        }

        return RedirectToAction("Chat", "Student", new { documentId, sessionId = chatSessionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(AskQuestionDto dto)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (accountId == null)
        {
            return RedirectToRoute("login");
        }

        try
        {
            var result = await _chatService.AskAsync(dto, accountId.Value);
            var role = HttpContext.Session.GetString("RoleName");
            if (role == "Teacher")
            {
                return RedirectToAction("Chat", "Teacher", new { documentId = result.DocumentId, sessionId = result.ChatSessionId });
            }

            return RedirectToAction("Chat", "Student", new { documentId = result.DocumentId, sessionId = result.ChatSessionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Ask", new { documentId = dto.DocumentId, chatSessionId = dto.ChatSessionId });
        }
    }

    private IActionResult RedirectToRoleChat()
    {
        var role = HttpContext.Session.GetString("RoleName");
        if (role == "Teacher")
        {
            return RedirectToAction("Chat", "Teacher");
        }

        return RedirectToAction("Chat", "Student");
    }
}
