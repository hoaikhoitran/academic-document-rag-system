using AcademicDocumentRagSystem.Services.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Interfaces
{
    public interface IAccountService
    {
        Task<LoginResultDto> LoginAsync(LoginDto dto);
    }
}
