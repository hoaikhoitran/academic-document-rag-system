namespace AcademicDocumentRagSystem.Services.DTOs.Accounts;

public class AccountListItemDto
{
    public int AccountId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int Role { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    public string? CourseCode { get; set; }

    public string? CourseName { get; set; }

    public bool Status { get; set; }
}
