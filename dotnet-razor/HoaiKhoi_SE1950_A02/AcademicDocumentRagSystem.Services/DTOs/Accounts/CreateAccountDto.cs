using System.ComponentModel.DataAnnotations;

namespace AcademicDocumentRagSystem.Services.DTOs.Accounts;

public class CreateAccountDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public int Role { get; set; } = 1;

    public int? CourseId { get; set; }

    public bool Status { get; set; } = true;
}
