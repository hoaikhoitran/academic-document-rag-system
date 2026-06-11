using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Documents
{
    [SessionAuthorize("Teacher")]
    public class IndexModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public IndexModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public List<DocumentListItemDto> Documents { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);

            if (accountId is null)
            {
                return RedirectToPage("/Auth/Login");
            }

            Documents = await _documentService.GetByTeacherAsync(accountId.Value);
            return Page();
        }
    }
}
