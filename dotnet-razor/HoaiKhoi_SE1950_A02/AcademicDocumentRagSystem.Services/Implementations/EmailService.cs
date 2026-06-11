using System.Net;
using System.Net.Mail;
using AcademicDocumentRagSystem.Services.Email;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AcademicDocumentRagSystem.Services.Implementations
{
    /// <summary>
    /// SMTP email sender backed by <see cref="SmtpClient"/>.
    /// All SMTP settings come from the "Smtp" configuration section so that no
    /// host, credential, or secret is ever hard-coded in the codebase.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _settings = ReadSettings(configuration);
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default)
        {
            if (!_settings.IsConfigured)
            {
                throw new InvalidOperationException(
                    "SMTP is not configured. Populate the 'Smtp' section (Host, UserName, Password, FromEmail) " +
                    "in appsettings or user-secrets before sending email.");
            }

            var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail)
                ? _settings.UserName
                : _settings.FromEmail;

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = string.IsNullOrWhiteSpace(_settings.UserName)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_settings.UserName, _settings.Password)
            };

            _logger.LogInformation("Sending email to {ToEmail} via {Host}:{Port}.", toEmail, _settings.Host, _settings.Port);
            await client.SendMailAsync(message, cancellationToken);
        }

        private static SmtpSettings ReadSettings(IConfiguration configuration)
        {
            var section = configuration.GetSection(SmtpSettings.SectionName);

            var settings = new SmtpSettings
            {
                Host = section["Host"] ?? string.Empty,
                UserName = section["UserName"] ?? string.Empty,
                Password = section["Password"] ?? string.Empty,
                FromEmail = section["FromEmail"] ?? string.Empty,
                FromName = string.IsNullOrWhiteSpace(section["FromName"])
                    ? "Academic Document RAG System"
                    : section["FromName"]!
            };

            if (int.TryParse(section["Port"], out var port))
            {
                settings.Port = port;
            }

            if (bool.TryParse(section["EnableSsl"], out var enableSsl))
            {
                settings.EnableSsl = enableSsl;
            }

            return settings;
        }
    }
}
