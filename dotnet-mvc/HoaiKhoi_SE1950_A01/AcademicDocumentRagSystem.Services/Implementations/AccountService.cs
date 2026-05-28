using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using AcademicDocumentRagSystem.Services.DTOs.Auth;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IConfiguration _configuration;

        public AccountService(
            IAccountRepository accountRepository,
            IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
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
                RoleName = account.Role == 1 ? "Student" : "Teacher"
            };
        }
    }
}
