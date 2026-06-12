using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Documents
{
    [SessionAuthorize("Teacher", "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public DetailsModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public DocumentDetailsDto Details { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);
            var roleName = HttpContext.Session.GetString(SessionKeys.RoleName) ?? string.Empty;

            var details = await _documentService.GetDetailsAsync(id, accountId, roleName);

            if (details is null)
            {
                return RedirectToPage("/AccessDenied");
            }

            Details = details;
            return Page();
        }

        public async Task<IActionResult> OnPostReindexAsync(int id)
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);
            var email = HttpContext.Session.GetString(SessionKeys.Email) ?? string.Empty;
            var roleName = HttpContext.Session.GetString(SessionKeys.RoleName) ?? string.Empty;

            try
            {
                await _documentService.ReIndexAsync(id, accountId, email, roleName);
                TempData["Success"] = "Document re-indexed. Chunk preview was rebuilt.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage("/Documents/Details", new { id });
        }
    }
}
