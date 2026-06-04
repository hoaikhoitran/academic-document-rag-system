using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.MVC.ViewModels;
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

    private void SetSidebar()
    {
        ViewBag.SidebarAccent = "Sinh viên";
        ViewBag.SidebarRole = "Student";
        ViewBag.NavItems = SidebarNav.Student;
    }

    public async Task<IActionResult> Chat(int? documentId, int? sessionId)
    {
        SetSidebar();
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        var workspace = await _chatService.GetWorkspaceAsync(accountId.Value, documentId, sessionId);
        workspace.SuccessMessage = TempData["Success"] as string;
        workspace.ErrorMessage = TempData["Error"] as string;
        return View(workspace);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(AskQuestionDto dto)
    {
        SetSidebar();
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            var invalidWorkspace = await _chatService.GetWorkspaceAsync(accountId.Value, dto.DocumentId, dto.ChatSessionId);
            invalidWorkspace.AskForm = dto;
            invalidWorkspace.ErrorMessage = "Vui lòng nhập câu hỏi.";
            return View("Chat", invalidWorkspace);
        }

        try
        {
            var result = await _chatService.AskAsync(dto, accountId.Value);
            TempData["Success"] = "Đã nhận câu trả lời từ RAG.";
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

    public async Task<IActionResult> Library(string? course)
    {
        SetSidebar();
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

        ViewBag.CourseCode = courseCode;
        ViewBag.FilterCourse = course;
        ViewBag.AllCourseCodes = all.Select(d => d.CourseCode).Distinct().OrderBy(c => c).ToList();
        return View(docs.ToList());
    }

    public IActionResult Document()
    {
        return RedirectToAction(nameof(Library));
    }

    public async Task<IActionResult> Document(int id)
    {
        SetSidebar();
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var details = await _documentService.GetDetailsAsync(id, accountId, "Student");
        if (details == null)
        {
            return NotFound();
        }

        return View(details);
    }

    public IActionResult Settings()
    {
        SetSidebar();
        return View();
    }

    private int? RequireAccountId()
    {
        return HttpContext.Session.GetInt32("AccountId");
    }
}
