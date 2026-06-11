using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Chat
{
    [SessionAuthorize("Teacher", "Student")]
    public class SessionModel : PageModel
    {
        private readonly IChatService _chatService;

        public SessionModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public ChatSessionDetailsDto Session { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);

            if (accountId is null)
            {
                return RedirectToPage("/Auth/Login");
            }

            var session = await _chatService.GetSessionAsync(id, accountId.Value);

            if (session is null)
            {
                return RedirectToPage("/Chat/Sessions");
            }

            Session = session;
            return Page();
        }
    }
}
