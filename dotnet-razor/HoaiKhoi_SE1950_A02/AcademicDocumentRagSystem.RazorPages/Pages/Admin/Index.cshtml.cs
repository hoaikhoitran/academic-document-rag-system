using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Admin
{
    [SessionAuthorize("Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IAccountService _accountService;

        public IndexModel(ICourseService courseService, IAccountService accountService)
        {
            _courseService = courseService;
            _accountService = accountService;
        }

        public int CourseCount { get; private set; }
        public int ActiveCourseCount { get; private set; }
        public int AccountCount { get; private set; }
        public int TeacherCount { get; private set; }

        public async Task OnGetAsync()
        {
            var courses = await _courseService.GetAllAsync();
            CourseCount = courses.Count;
            ActiveCourseCount = courses.Count(c => c.Status);

            var accounts = await _accountService.GetAllAsync(null, null, null);
            AccountCount = accounts.Count;
            TeacherCount = accounts.Count(a => a.Role == 2);
        }
    }
}
