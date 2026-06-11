using AcademicDocumentRagSystem.RazorPages.Infrastructure;
using AcademicDocumentRagSystem.Services.DTOs.Accounts;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicDocumentRagSystem.RazorPages.Pages.Accounts
{
    [SessionAuthorize("Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly ICourseService _courseService;

        public IndexModel(IAccountService accountService, ICourseService courseService)
        {
            _accountService = accountService;
            _courseService = courseService;
        }

        public List<AccountListItemDto> Accounts { get; private set; } = new();
        public List<SelectListItem> Courses { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Role { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? Status { get; set; }

        [BindProperty]
        public CreateAccountDto CreateInput { get; set; } = new();

        [BindProperty]
        public UpdateAccountDto EditInput { get; set; } = new();

        public bool ShowCreateModal { get; private set; }
        public bool ShowEditModal { get; private set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Two bound models live on this page (CreateInput + EditInput). Validate
            // only the one this handler owns so the empty Edit model cannot block create.
            ModelState.Clear();

            if (!TryValidateModel(CreateInput, nameof(CreateInput)))
            {
                ShowCreateModal = true;
                await LoadAsync();
                return Page();
            }

            try
            {
                var result = await _accountService.CreateAsync(CreateInput);

                if (result.EmailAttempted && !result.EmailSent)
                {
                    // Account was created successfully, but the onboarding email failed.
                    // Surface a clear warning instead of failing the whole operation.
                    TempData["Warning"] =
                        $"Account '{CreateInput.Email}' was created, but the notification email could not be sent. " +
                        $"Reason: {result.EmailError}";
                }
                else if (result.EmailSent)
                {
                    TempData["Success"] =
                        $"Lecturer account '{CreateInput.Email}' was created and the credentials email was sent.";
                }
                else
                {
                    TempData["Success"] = $"Account '{CreateInput.Email}' was created.";
                }

                return RedirectToPage(new { SearchTerm, Role, Status });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CreateInput.Email", ex.Message);
                ShowCreateModal = true;
                await LoadAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(EditInput, nameof(EditInput)))
            {
                ShowEditModal = true;
                await LoadAsync();
                return Page();
            }

            try
            {
                await _accountService.UpdateAsync(EditInput);
                TempData["Success"] = $"Account '{EditInput.Email}' was updated.";
                return RedirectToPage(new { SearchTerm, Role, Status });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("EditInput.Email", ex.Message);
                ShowEditModal = true;
                await LoadAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _accountService.DeleteAsync(id);
                TempData["Success"] = "Account was deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SearchTerm, Role, Status });
        }

        private async Task LoadAsync()
        {
            Accounts = await _accountService.GetAllAsync(SearchTerm, Role, Status);

            var courses = await _courseService.GetAllAsync();
            Courses = courses
                .Where(c => c.Status)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToList();
        }
    }
}
