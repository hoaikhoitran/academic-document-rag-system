using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Student
{
    [SessionAuthorize("Student")]
    public class IndexModel : PageModel
    {
        public string? FullName { get; private set; }

        public void OnGet()
        {
            FullName = HttpContext.Session.GetString(SessionKeys.FullName);
        }
    }
}
