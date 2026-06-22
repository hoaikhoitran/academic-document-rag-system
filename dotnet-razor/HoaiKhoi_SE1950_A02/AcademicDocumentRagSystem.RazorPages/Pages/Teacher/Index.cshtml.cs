using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Teacher
{
    [SessionAuthorize("Teacher")]
    public class IndexModel : PageModel
    {
        public string? FullName { get; private set; }
        public string? CourseCode { get; private set; }
        public string? CourseName { get; private set; }

        /// <summary>
        /// Assigned course shown on the dashboard, built from session data only:
        /// "CODE - NAME" when both are present, otherwise "CODE", otherwise null
        /// (the page renders a friendly fallback when no course is assigned).
        /// </summary>
        public string? CourseDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CourseCode) && !string.IsNullOrWhiteSpace(CourseName))
                {
                    return $"{CourseCode} - {CourseName}";
                }

                if (!string.IsNullOrWhiteSpace(CourseCode))
                {
                    return CourseCode;
                }

                return string.IsNullOrWhiteSpace(CourseName) ? null : CourseName;
            }
        }

        public void OnGet()
        {
            FullName = HttpContext.Session.GetString(SessionKeys.FullName);
            CourseCode = HttpContext.Session.GetString(SessionKeys.CourseCode);
            CourseName = HttpContext.Session.GetString(SessionKeys.CourseName);
        }
    }
}
