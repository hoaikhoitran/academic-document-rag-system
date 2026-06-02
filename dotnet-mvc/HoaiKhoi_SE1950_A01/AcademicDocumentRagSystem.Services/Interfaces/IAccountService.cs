using AcademicDocumentRagSystem.Services.DTOs.Accounts;
using AcademicDocumentRagSystem.Services.DTOs.Auth;

namespace AcademicDocumentRagSystem.Services.Interfaces
{
    public interface IAccountService
    {
        Task<LoginResultDto> LoginAsync(LoginDto dto);

        Task<List<AccountListItemDto>> GetAllAsync(string? searchTerm, int? role, bool? status);

        Task<UpdateAccountDto?> GetForEditAsync(int id);

        Task CreateAsync(CreateAccountDto dto);

        Task UpdateAsync(UpdateAccountDto dto);

        Task DeleteAsync(int id);
    }
}
