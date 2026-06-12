using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Chat
{
    [SessionAuthorize("Teacher", "Student")]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;

        public IndexModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public List<IndexedDocumentDto> Documents { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Documents = await _chatService.GetIndexedDocumentsAsync();
        }
    }
}
