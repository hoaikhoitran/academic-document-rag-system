namespace AcademicDocumentRagSystem.Services.Email
{
    /// <summary>
    /// Strongly typed view of the "Smtp" configuration section.
    /// Values are supplied by appsettings.json / user-secrets, never hard-coded.
    /// </summary>
    public class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = "Academic Document RAG System";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
    }
}
