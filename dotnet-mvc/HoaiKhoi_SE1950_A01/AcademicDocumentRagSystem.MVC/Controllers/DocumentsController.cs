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

    public IActionResult Upload()
    {
        return View(new DocumentUploadDto
        {
            CourseId = 1,
            CourseCode = "PRN222"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Upload(DocumentUploadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var accountId = HttpContext.Session.GetInt32("AccountId");
        var email = HttpContext.Session.GetString("Email");

        if (accountId == null || string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            await _documentService.UploadAndIndexAsync(dto, accountId.Value, email);

            TempData["Success"] = "Document uploaded and indexed successfully.";

            return RedirectToAction("Upload");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
}