namespace AcademicDocumentRagSystem.Services.Interfaces
{
    /// <summary>
    /// Sends transactional email through the configured SMTP server.
    /// Implementations must throw on failure so callers can decide how to react
    /// (e.g. surface a warning) without corrupting the calling operation.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default);
    }
}
