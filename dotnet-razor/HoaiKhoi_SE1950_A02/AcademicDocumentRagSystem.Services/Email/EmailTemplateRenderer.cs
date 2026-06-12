using System.Collections.Concurrent;
using System.Net;
using AcademicDocumentRagSystem.Services.Email.Models;

namespace AcademicDocumentRagSystem.Services.Email
{
    /// <summary>
    /// Loads HTML email templates that ship as embedded resources and substitutes
    /// <c>{{Token}}</c> placeholders with HTML-encoded model values. Templates are
    /// cached after first load. Adding a new email is just a new template + model.
    /// </summary>
    public class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private const string TeacherWelcomeResource =
            "AcademicDocumentRagSystem.Services.Email.Templates.TeacherWelcome.html";

        private readonly ConcurrentDictionary<string, string> _templateCache = new();

        public string RenderTeacherWelcome(TeacherWelcomeEmailModel model)
        {
            var template = LoadTemplate(TeacherWelcomeResource);

            var values = new Dictionary<string, string>
            {
                ["TeacherName"] = WebUtility.HtmlEncode(model.TeacherName),
                ["Email"] = WebUtility.HtmlEncode(model.Email),
                ["Password"] = WebUtility.HtmlEncode(model.Password),
                ["CourseName"] = WebUtility.HtmlEncode(
                    string.IsNullOrWhiteSpace(model.CourseName) ? "Not assigned" : model.CourseName),
                ["LoginUrl"] = WebUtility.HtmlEncode(model.LoginUrl),
                ["CurrentYear"] = model.CurrentYear.ToString()
            };

            return Apply(template, values);
        }

        private string LoadTemplate(string resourceName)
        {
            return _templateCache.GetOrAdd(resourceName, name =>
            {
                var assembly = typeof(EmailTemplateRenderer).Assembly;
                using var stream = assembly.GetManifestResourceStream(name)
                    ?? throw new InvalidOperationException(
                        $"Email template '{name}' was not found. Ensure it is included as an <EmbeddedResource>.");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            });
        }

        private static string Apply(string template, IReadOnlyDictionary<string, string> values)
        {
            foreach (var pair in values)
            {
                template = template.Replace("{{" + pair.Key + "}}", pair.Value);
            }

            return template;
        }
    }
}
