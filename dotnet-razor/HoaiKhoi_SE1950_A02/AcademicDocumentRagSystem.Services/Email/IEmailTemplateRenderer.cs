using AcademicDocumentRagSystem.Services.Email.Models;

namespace AcademicDocumentRagSystem.Services.Email
{
    /// <summary>
    /// Renders HTML email bodies from templates. Keeps template/markup concerns out
    /// of <see cref="Interfaces.IEmailService"/> (which only transports the email) and
    /// out of the business services (which only supply the data model).
    /// </summary>
    public interface IEmailTemplateRenderer
    {
        string RenderTeacherWelcome(TeacherWelcomeEmailModel model);
    }
}
