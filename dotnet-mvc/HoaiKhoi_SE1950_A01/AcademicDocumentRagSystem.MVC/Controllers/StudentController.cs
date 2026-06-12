using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Student")]
public class StudentController : Controller
{
    private readonly IChatService _chatService;
    private readonly IDocumentService _documentService;

    public StudentController(IChatService chatService, IDocumentService documentService)
    {
        _chatService = chatService;
        _documentService = documentService;
    }

    private async Task LoadSidebarAsync(int? selectedSessionId = null)
    {
        var accountId = RequireAccountId();
        ViewBag.RecentSessions = accountId.HasValue
            ? await _chatService.GetSessionsAsync(accountId.Value)
            : new List<ChatSessionDto>();
        ViewBag.SelectedSessionId = selectedSessionId;
    }

    public async Task<IActionResult> Chat(int? documentId, int? sessionId)
    {
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        var workspace = await _chatService.GetWorkspaceAsync(accountId.Value, documentId, sessionId);
        workspace.SuccessMessage = TempData["Success"] as string;
        workspace.ErrorMessage = TempData["Error"] as string;
        ViewBag.RecentSessions = workspace.Sessions;
        ViewBag.SelectedSessionId = workspace.SelectedSessionId;
        return View(workspace);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(AskQuestionDto dto)
    {
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            var invalidWorkspace = await _chatService.GetWorkspaceAsync(accountId.Value, dto.DocumentId, dto.ChatSessionId);
            invalidWorkspace.AskForm = dto;
            invalidWorkspace.ErrorMessage = "Vui lòng nhập câu hỏi.";
            ViewBag.RecentSessions = invalidWorkspace.Sessions;
            ViewBag.SelectedSessionId = invalidWorkspace.SelectedSessionId;
            return View("Chat", invalidWorkspace);
        }

        try
        {
            var result = await _chatService.AskAsync(dto, accountId.Value);
            return RedirectToAction(nameof(Chat), new { documentId = result.DocumentId, sessionId = result.ChatSessionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Chat), new { documentId = dto.DocumentId, sessionId = dto.ChatSessionId });
        }
    }

    public IActionResult NewChat(int documentId)
    {
        return RedirectToAction(nameof(Chat), new { documentId });
    }

    public async Task<IActionResult> Library(string? course, string? q)
    {
        await LoadSidebarAsync();
        var all = await _chatService.GetIndexedDocumentsAsync();
        var courseId = HttpContext.Session.GetInt32("CourseId");
        var courseCode = HttpContext.Session.GetString("CourseCode");

        var docs = all.AsEnumerable();
        if (courseId.HasValue)
        {
            docs = docs.Where(d => string.Equals(d.CourseCode, courseCode, StringComparison.OrdinalIgnoreCase)
                || (course != null && string.Equals(d.CourseCode, course, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(course))
        {
            docs = docs.Where(d => string.Equals(d.CourseCode, course, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            docs = docs.Where(d => d.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (d.Chapter?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        ViewBag.CourseCode = courseCode;
        ViewBag.FilterCourse = course;
        ViewBag.SearchQuery = q;
        ViewBag.AllCourseCodes = all.Select(d => d.CourseCode).Distinct().OrderBy(c => c).ToList();
        return View(docs.ToList());
    }

    public async Task<IActionResult> Document(int? id)
    {
        if (!id.HasValue)
        {
            return RedirectToAction(nameof(Library));
        }

        await LoadSidebarAsync();
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var details = await _documentService.GetDetailsAsync(id.Value, accountId, "Student");
        if (details == null)
        {
            return NotFound();
        }

        return View(details);
    }

    public async Task<IActionResult> Settings()
    {
        await LoadSidebarAsync();
        return View();
    }

    private int? RequireAccountId()
    {
        return HttpContext.Session.GetInt32("AccountId");
    }
}
