using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Documents
{
    [SessionAuthorize("Teacher")]
    public class UploadModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public UploadModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [BindProperty]
        public DocumentUploadDto Input { get; set; } = new();

        public List<CourseDto> AvailableCourses { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);
            var courseId = HttpContext.Session.GetInt32(SessionKeys.CourseId);
            var courseCode = HttpContext.Session.GetString(SessionKeys.CourseCode);

            if (accountId is null || courseId is null || string.IsNullOrWhiteSpace(courseCode))
            {
                return RedirectToPage("/AccessDenied");
            }

            Input.CourseId = courseId.Value;
            Input.CourseCode = courseCode;
            AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);
            var email = HttpContext.Session.GetString(SessionKeys.Email);

            if (accountId is null || string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
                return Page();
            }

            try
            {
                // CourseId comes from the form and is re-validated in the service against
                // the teacher's assigned course; it is never silently overwritten.
                var documentId = await _documentService.UploadAndIndexAsync(Input, accountId.Value, email);
                TempData["Success"] = "Document uploaded and indexed. Below is how it was split into chunks.";
                return RedirectToPage("/Documents/Details", new { id = documentId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                AvailableCourses = await _documentService.GetUploadCoursesForTeacherAsync(accountId.Value);
                return Page();
            }
        }
    }
}
