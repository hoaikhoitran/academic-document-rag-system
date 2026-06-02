using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Teacher")]
public class DocumentsController : Controller
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

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

    public IActionResult Upload()
    {
        var courseId = HttpContext.Session.GetInt32("CourseId");
        var courseCode = HttpContext.Session.GetString("CourseCode");

        if (courseId == null || string.IsNullOrWhiteSpace(courseCode))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View(new DocumentUploadDto
        {
            CourseId = courseId.Value,
            CourseCode = courseCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadDto dto)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        var email = HttpContext.Session.GetString("Email");
        var courseId = HttpContext.Session.GetInt32("CourseId");
        var courseCode = HttpContext.Session.GetString("CourseCode");

        if (accountId == null || string.IsNullOrWhiteSpace(email) || courseId == null || string.IsNullOrWhiteSpace(courseCode))
        {
            return RedirectToAction("Login", "Auth");
        }

        dto.CourseId = courseId.Value;
        dto.CourseCode = courseCode;

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _documentService.UploadAndIndexAsync(dto, accountId.Value, email);

            TempData["Success"] = "Document uploaded and indexed successfully.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
}
