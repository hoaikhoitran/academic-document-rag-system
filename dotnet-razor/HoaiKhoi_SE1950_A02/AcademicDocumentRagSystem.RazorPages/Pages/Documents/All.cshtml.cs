using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.DTOs.Documents;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Documents
{
    [SessionAuthorize("Admin")]
    public class AllModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public AllModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public List<DocumentListItemDto> Documents { get; private set; } = new();
        public List<CourseDto> Courses { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CourseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? IndexStatus { get; set; }

        public async Task OnGetAsync()
        {
            var filter = new DocumentFilterDto
            {
                CourseId = CourseId,
                IndexStatus = string.IsNullOrWhiteSpace(IndexStatus) ? null : IndexStatus
            };

            Documents = await _documentService.GetAllForAdminAsync(filter);
            Courses = await _documentService.GetCourseFilterOptionsAsync();
        }
    }
}
