using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.MVC.ViewModels;
using AcademicDocumentRagSystem.MVC.ViewModels.Mock;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Teacher")]
public class TeacherController : Controller
{
    private readonly IChatService _chatService;
    private readonly IDocumentService _documentService;

    public TeacherController(
        IChatService chatService,
        IDocumentService documentService)
    {
        _chatService = chatService;
        _documentService = documentService;
    }

    private void SetSidebar()
    {
        ViewBag.SidebarAccent = "Giảng viên";
        ViewBag.SidebarRole = "Teacher";
        ViewBag.NavItems = SidebarNav.Teacher;
    }

    public async Task<IActionResult> Courses()
    {
        SetSidebar();
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        var courses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
        var documents = await _documentService.GetByTeacherAsync(accountId.Value);

        var cards = courses.Select(c => new TeacherCourseCardViewModel
        {
            Course = c,
            DocumentCount = documents.Count(d => d.CourseCode == c.Code),
            IndexedCount = documents.Count(d => d.CourseCode == c.Code && d.IndexStatus == "Indexed"),
            TotalChunks = documents.Where(d => d.CourseCode == c.Code).Sum(d => d.TotalChunks)
        }).ToList();

        return View(cards);
    }

    public async Task<IActionResult> Upload()
    {
        SetSidebar();
        var accountId = RequireAccountId();
        var courseId = HttpContext.Session.GetInt32("CourseId");
        var courseCode = HttpContext.Session.GetString("CourseCode");

        if (accountId == null || courseId == null || string.IsNullOrWhiteSpace(courseCode))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        var model = new DocumentUploadDto
        {
            CourseId = courseId.Value,
            CourseCode = courseCode,
            AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadDto dto)
    {
        SetSidebar();
        var accountId = RequireAccountId();
        var email = HttpContext.Session.GetString("Email");

        if (accountId == null || string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            dto.AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
            return View(dto);
        }

        try
        {
            var documentId = await _documentService.UploadAndIndexAsync(dto, accountId.Value, email);
            TempData["Success"] = "Tài liệu đã upload và đang được index.";
            return RedirectToAction(nameof(IndexStatus), new { highlight = documentId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            dto.AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
            return View(dto);
        }
    }

    public async Task<IActionResult> Library(string? course, string? q)
    {
        SetSidebar();
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        var all = await _documentService.GetByTeacherAsync(accountId.Value);
        var docs = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(course))
        {
            docs = docs.Where(d => string.Equals(d.CourseCode, course, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            docs = docs.Where(d => d.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (d.Chapter?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (d.OriginalFileName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        ViewBag.FilterCourse = course;
        ViewBag.SearchQuery = q;
        ViewBag.AllCourseCodes = all.Select(d => d.CourseCode).Distinct().OrderBy(c => c).ToList();
        return View(docs.ToList());
    }

    public IActionResult Settings()
    {
        SetSidebar();
        return View();
    }

    public IActionResult NewChat(int documentId)
    {
        return RedirectToAction(nameof(Chat), new { documentId });
    }

    public async Task<IActionResult> IndexStatus(int? highlight)
    {
        SetSidebar();
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        var documents = await _documentService.GetByTeacherAsync(accountId.Value);
        ViewBag.HighlightId = highlight;
        ViewBag.Success = TempData["Success"];
        ViewBag.Error = TempData["Error"];
        return View(documents);
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
        var accountId = RequireAccountId();
        if (accountId == null) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng nhập câu hỏi.";
            return RedirectToAction(nameof(Chat), new { documentId = dto.DocumentId, sessionId = dto.ChatSessionId });
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

    public IActionResult Overview()
    {
        SetSidebar();
        return View(MockDataProvider.GetAdminDashboardViewModel());
    }

    public async Task<IActionResult> DocumentDetails(int id)
    {
        SetSidebar();
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var details = await _documentService.GetDetailsAsync(id, accountId, "Teacher");
        if (details == null)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View(details);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReIndex(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var email = HttpContext.Session.GetString("Email") ?? string.Empty;

        try
        {
            await _documentService.ReIndexAsync(id, accountId, email, "Teacher");
            TempData["Success"] = "Đã re-index tài liệu.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(IndexStatus), new { highlight = id });
    }

    private int? RequireAccountId() => HttpContext.Session.GetInt32("AccountId");
}
