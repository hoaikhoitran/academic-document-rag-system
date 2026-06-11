using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Chat;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Chat
{
    [SessionAuthorize("Teacher", "Student")]
    public class AskModel : PageModel
    {
        private readonly IChatService _chatService;

        public AskModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        [BindProperty]
        public AskQuestionDto Input { get; set; } = new();

        public ChatAnswerDto? Answer { get; private set; }
        public string? RagError { get; private set; }

        public void OnGet(int documentId, int? chatSessionId)
        {
            Input.DocumentId = documentId;
            Input.ChatSessionId = chatSessionId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var accountId = HttpContext.Session.GetInt32(SessionKeys.AccountId);

            if (accountId is null)
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                Answer = await _chatService.AskAsync(Input, accountId.Value);
                // Keep the session id so follow-up questions stay in the same thread.
                Input.ChatSessionId = Answer.ChatSessionId;
            }
            catch (Exception ex)
            {
                // RAG service offline / errored: fail gracefully without crashing.
                RagError =
                    "The RAG service could not answer this question right now. " +
                    "Please make sure the RAG service is running and try again. Details: " + ex.Message;
            }

            return Page();
        }
    }
}
