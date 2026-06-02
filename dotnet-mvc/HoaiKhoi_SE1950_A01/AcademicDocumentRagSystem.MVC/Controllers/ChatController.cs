using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Student", "Teacher")]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _chatService.GetIndexedDocumentsAsync();
        return View(documents);
    }

    public async Task<IActionResult> Sessions()
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");

        if (accountId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var sessions = await _chatService.GetSessionsAsync(accountId.Value);
        return View(sessions);
    }

    public async Task<IActionResult> Session(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");

        if (accountId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var session = await _chatService.GetSessionAsync(id, accountId.Value);

        if (session == null)
        {
            return NotFound();
        }

        return View(session);
    }

    public IActionResult Ask(int documentId, int? chatSessionId)
    {
        return View(new AskQuestionDto
        {
            DocumentId = documentId,
            ChatSessionId = chatSessionId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(AskQuestionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var accountId = HttpContext.Session.GetInt32("AccountId");

        if (accountId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var result = await _chatService.AskAsync(dto, accountId.Value);
            return View("Answer", result);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
}
