using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Teacher
{
    [SessionAuthorize("Teacher")]
    public class CoursesModel : PageModel
    {
        private readonly ICourseService _courseService;

        public CoursesModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public List<CourseDto> Courses { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Courses = await _courseService.GetAllAsync();
        }

        // AJAX endpoint used by course-realtime.js to re-fetch just the table
        // fragment when the SignalR hub signals that courses changed.
        public async Task<IActionResult> OnGetTableAsync()
        {
            var courses = await _courseService.GetAllAsync();
            return Partial("_CourseTable", courses);
        }
    }
}
