using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

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

    public IActionResult Ask(int documentId)
    {
        return View(new AskQuestionDto
        {
            DocumentId = documentId
        });
    }

    [HttpPost]
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