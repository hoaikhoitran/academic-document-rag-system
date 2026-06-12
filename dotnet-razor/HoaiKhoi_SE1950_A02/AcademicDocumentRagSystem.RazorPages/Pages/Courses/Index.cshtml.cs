using AcademicDocumentRagSystem.RazorPages.Hubs;
using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Courses
{
    [SessionAuthorize("Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IHubContext<CourseHub> _courseHub;

        public IndexModel(ICourseService courseService, IHubContext<CourseHub> courseHub)
        {
            _courseService = courseService;
            _courseHub = courseHub;
        }

        public List<CourseDto> Courses { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public CreateCourseDto CreateInput { get; set; } = new();

        [BindProperty]
        public UpdateCourseDto EditInput { get; set; } = new();

        // When server-side validation fails we re-render the page with the relevant
        // modal re-opened so the admin sees the error without losing their input.
        public bool ShowCreateModal { get; private set; }
        public bool ShowEditModal { get; private set; }

        public async Task OnGetAsync()
        {
            await LoadCoursesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // This page hosts two bound models (CreateInput + EditInput). Posting the
            // Create form leaves EditInput empty, which would otherwise fail its own
            // [Required]/non-nullable validation and silently block the create. So we
            // clear all model state and validate ONLY the model this handler owns.
            ModelState.Clear();

            if (!TryValidateModel(CreateInput, nameof(CreateInput)))
            {
                ShowCreateModal = true;
                await LoadCoursesAsync();
                return Page();
            }

            try
            {
                await _courseService.CreateAsync(CreateInput);

                // Broadcast only after the service-layer write succeeds.
                await _courseHub.Clients.All.SendAsync(
                    CourseHub.CourseCreated, new { CreateInput.Code, CreateInput.Name });
                await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged);

                TempData["Success"] = $"Course '{CreateInput.Code}' was created.";
                return RedirectToPage(new { SearchTerm });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CreateInput.Code", ex.Message);
                ShowCreateModal = true;
                await LoadCoursesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(EditInput, nameof(EditInput)))
            {
                ShowEditModal = true;
                await LoadCoursesAsync();
                return Page();
            }

            try
            {
                await _courseService.UpdateAsync(EditInput);

                await _courseHub.Clients.All.SendAsync(
                    CourseHub.CourseUpdated, new { EditInput.CourseId, EditInput.Code, EditInput.Name });
                await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged);

                TempData["Success"] = $"Course '{EditInput.Code}' was updated.";
                return RedirectToPage(new { SearchTerm });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("EditInput.Code", ex.Message);
                ShowEditModal = true;
                await LoadCoursesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _courseService.DeleteAsync(id);

                await _courseHub.Clients.All.SendAsync(CourseHub.CourseDeleted, new { CourseId = id });
                await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged);

                TempData["Success"] = "Course was deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SearchTerm });
        }

        private async Task LoadCoursesAsync()
        {
            Courses = await _courseService.SearchAsync(SearchTerm);
        }
    }
}
