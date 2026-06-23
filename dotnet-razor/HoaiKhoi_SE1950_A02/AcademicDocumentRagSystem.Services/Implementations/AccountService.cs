using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using AcademicDocumentRagSystem.Services.DTOs.Accounts;
using AcademicDocumentRagSystem.Services.DTOs.Auth;
using AcademicDocumentRagSystem.Services.Email;
using AcademicDocumentRagSystem.Services.Email.Models;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AcademicDocumentRagSystem.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            ICourseRepository courseRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IEmailTemplateRenderer emailTemplateRenderer,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _courseRepository = courseRepository;
            _configuration = configuration;
            _emailService = emailService;
            _emailTemplateRenderer = emailTemplateRenderer;
            _logger = logger;
        }

        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];

            if (dto.Email == adminEmail && dto.Password == adminPassword)
            {
                return new LoginResultDto
                {
                    IsSuccess = true,
                    AccountId = null,
                    Email = dto.Email,
                    FullName = "System Admin",
                    RoleName = "Admin",
                    Role = null
                };
            }

            var account = await _accountRepository.GetByEmailAndPasswordAsync(dto.Email, dto.Password);

            if (account == null)
            {
                return new LoginResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            return new LoginResultDto
            {
                IsSuccess = true,
                AccountId = account.AccountId,
                Email = account.Email,
                FullName = account.FullName,
                Role = account.Role,
                RoleName = GetRoleName(account.Role),
                CourseId = account.CourseId,
                CourseCode = account.Course?.Code,
                CourseName = account.Course?.Name
            };
        }

        public async Task<List<AccountListItemDto>> GetAllAsync(string? searchTerm, int? role, bool? status)
        {
            var accounts = await _accountRepository.GetAllAsync(searchTerm, role, status);

            return accounts.Select(a => new AccountListItemDto
            {
                AccountId = a.AccountId,
                Email = a.Email,
                FullName = a.FullName,
                Role = a.Role,
                RoleName = GetRoleName(a.Role),
                CourseId = a.CourseId,
                CourseCode = a.Course?.Code,
                CourseName = a.Course?.Name,
                Status = a.Status
            }).ToList();
        }

        public async Task<UpdateAccountDto?> GetForEditAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account == null)
            {
                return null;
            }

            return new UpdateAccountDto
            {
                AccountId = account.AccountId,
                Email = account.Email,
                FullName = account.FullName,
                Role = account.Role,
                CourseId = account.CourseId,
                Status = account.Status
            };
        }

        public async Task<CreateAccountResult> CreateAsync(CreateAccountDto dto)
        {
            await ValidateAccountAsync(dto.Email, dto.Role, dto.CourseId, null);

            var account = new Account
            {
                Email = dto.Email.Trim(),
                Password = dto.Password,
                FullName = dto.FullName.Trim(),
                Role = dto.Role,
                CourseId = dto.Role == 2 ? dto.CourseId : null,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveChangesAsync();

            var result = new CreateAccountResult { AccountId = account.AccountId };

            // Onboarding email is only sent for Teacher/Lecturer accounts. Email
            // delivery must never corrupt or roll back the already-persisted account,
            // so failures are caught, logged, and surfaced via the result instead.
            if (account.Role == 2)
            {
                result.EmailAttempted = true;

                try
                {
                    await SendAccountCreatedEmailAsync(account, dto.Password);
                    result.EmailSent = true;
                }
                catch (Exception ex)
                {
                    result.EmailSent = false;
                    result.EmailError = ex.Message;
                    _logger.LogError(
                        ex,
                        "Lecturer account {Email} (id {AccountId}) was created but the onboarding email failed to send.",
                        account.Email,
                        account.AccountId);
                }
            }

            return result;
        }

        private async Task SendAccountCreatedEmailAsync(Account account, string temporaryPassword)
        {
            var courseName = "Not assigned";

            if (account.CourseId.HasValue)
            {
                var course = await _courseRepository.GetByIdAsync(account.CourseId.Value);

                if (course != null)
                {
                    courseName = $"{course.Code} - {course.Name}";
                }
            }

            var model = new TeacherWelcomeEmailModel
            {
                TeacherName = account.FullName,
                Email = account.Email,
                Password = temporaryPassword,
                CourseName = courseName,
                LoginUrl = _configuration["App:LoginUrl"] ?? string.Empty,
                CurrentYear = DateTime.UtcNow.Year
            };

            // The service composes the data model and renders the premium HTML
            // template; IEmailService only transports the resulting message.
            var htmlBody = _emailTemplateRenderer.RenderTeacherWelcome(model);

            await _emailService.SendEmailAsync(
                account.Email,
                "Welcome to Academic Document RAG System",
                htmlBody);
        }

        public async Task UpdateAsync(UpdateAccountDto dto)
        {
            var account = await _accountRepository.GetByIdAsync(dto.AccountId);

            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }

            await ValidateAccountAsync(dto.Email, dto.Role, dto.CourseId, dto.AccountId);

            account.Email = dto.Email.Trim();
            account.FullName = dto.FullName.Trim();
            account.Role = dto.Role;
            account.CourseId = dto.Role == 2 ? dto.CourseId : null;
            account.Status = dto.Status;
            account.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                account.Password = dto.Password;
            }

            _accountRepository.Update(account);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }

            _accountRepository.Delete(account);
            await _accountRepository.SaveChangesAsync();
        }

        private async Task ValidateAccountAsync(string email, int role, int? courseId, int? accountId)
        {
            if (role != 1 && role != 2)
            {
                throw new ArgumentException("Role must be Student or Teacher.");
            }

            var normalizedEmail = email.Trim();
            var adminEmail = _configuration["AdminAccount:Email"];

            if (!string.IsNullOrWhiteSpace(adminEmail) &&
                string.Equals(normalizedEmail, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("This email is reserved for the system admin account.");
            }

            var existingAccount = accountId.HasValue
                ? await _accountRepository.GetByEmailExceptIdAsync(normalizedEmail, accountId.Value)
                : await _accountRepository.GetByEmailAsync(normalizedEmail);

            if (existingAccount != null)
            {
                throw new ArgumentException("Email is already used by another account.");
            }

            if (role == 1 && courseId.HasValue)
            {
                throw new ArgumentException("Student accounts must not be assigned to a course.");
            }

            if (role == 2)
            {
                if (!courseId.HasValue)
                {
                    throw new ArgumentException("Teacher accounts must be assigned to a course.");
                }

                var course = await _courseRepository.GetByIdAsync(courseId.Value);

                if (course == null)
                {
                    throw new ArgumentException("Assigned course was not found.");
                }
            }
        }

        private static string GetRoleName(int role)
        {
            return role == 1 ? "Student" : "Teacher";
        }
    }
}
