namespace AcademicDocumentRagSystem.Services.Email.Models
{
    /// <summary>
    /// Data injected into the <c>TeacherWelcome.html</c> email template.
    /// Kept free of any presentation markup so the same model can drive future
    /// templates or localized variants.
    /// </summary>
    public class TeacherWelcomeEmailModel
    {
        public string TeacherName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string CourseName { get; set; } = "Not assigned";

        public string LoginUrl { get; set; } = string.Empty;

        public int CurrentYear { get; set; } = DateTime.UtcNow.Year;
    }
}
