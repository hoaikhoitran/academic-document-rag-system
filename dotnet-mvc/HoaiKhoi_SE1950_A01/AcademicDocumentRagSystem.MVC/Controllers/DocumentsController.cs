using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

public class DocumentsController : Controller
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [SessionAuthorize("Teacher")]
    public async Task<IActionResult> Index()
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");

        if (accountId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var documents = await _documentService.GetByTeacherAsync(accountId.Value);
        return View(documents);
    }

    [SessionAuthorize("Admin")]
    public async Task<IActionResult> All(DocumentFilterDto filter)
    {
        var documents = await _documentService.GetAllForAdminAsync(filter);

        ViewBag.Courses = await _documentService.GetCourseFilterOptionsAsync();
        ViewBag.Filter = filter;

        return View(documents);
    }

    [SessionAuthorize("Teacher")]
    public async Task<IActionResult> Upload()
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
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
    [SessionAuthorize("Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadDto dto)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var email = HttpContext.Session.GetString("Email");

        if (accountId == null || string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Login", "Auth");
        }

        // CourseId comes from the form (dropdown) and is validated in the service
        // against the teacher's assigned course. It is never silently overwritten.
        if (!ModelState.IsValid)
        {
            dto.AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
            return View(dto);
        }

        try
        {
            var documentId = await _documentService.UploadAndIndexAsync(dto, accountId.Value, email);

            TempData["Success"] = "Document uploaded and indexed. Below is how it was split into chunks.";

            return RedirectToAction(nameof(Details), new { id = documentId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            dto.AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
            return View(dto);
        }
    }

    [SessionAuthorize("Teacher", "Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var roleName = HttpContext.Session.GetString("RoleName") ?? string.Empty;

        var details = await _documentService.GetDetailsAsync(id, accountId, roleName);

        if (details == null)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View(details);
    }

    [HttpPost]
    [SessionAuthorize("Teacher", "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReIndex(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var email = HttpContext.Session.GetString("Email") ?? string.Empty;
        var roleName = HttpContext.Session.GetString("RoleName") ?? string.Empty;

        try
        {
            await _documentService.ReIndexAsync(id, accountId, email, roleName);
            TempData["Success"] = "Document re-indexed. Chunk preview was rebuilt.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
