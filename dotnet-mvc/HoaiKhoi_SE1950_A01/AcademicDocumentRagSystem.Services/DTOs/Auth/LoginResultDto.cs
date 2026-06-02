using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.DTOs.Auth
{
    public class LoginResultDto
    {
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        public int? AccountId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public int? Role { get; set; }

        public int? CourseId { get; set; }

        public string? CourseCode { get; set; }
    }
}
