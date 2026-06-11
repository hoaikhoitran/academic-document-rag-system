using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Teacher
{
    [SessionAuthorize("Teacher")]
    public class IndexModel : PageModel
    {
        public string? FullName { get; private set; }
        public string? CourseCode { get; private set; }

        public void OnGet()
        {
            FullName = HttpContext.Session.GetString(SessionKeys.FullName);
            CourseCode = HttpContext.Session.GetString(SessionKeys.CourseCode);
        }
    }
}
