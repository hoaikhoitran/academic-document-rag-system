using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Chat
{
    [SessionAuthorize("Teacher", "Student")]
    public class SessionsModel : PageModel
    {
        private readonly IChatService _chatService;

        public SessionsModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public List<ChatSessionDto> Sessions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);

            if (accountId is null)
            {
                return RedirectToPage("/Auth/Login");
            }

            Sessions = await _chatService.GetSessionsAsync(accountId.Value);
            return Page();
        }
    }
}
