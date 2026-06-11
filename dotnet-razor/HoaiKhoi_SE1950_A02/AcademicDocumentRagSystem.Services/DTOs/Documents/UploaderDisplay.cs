namespace AcademicDocumentRagSystem.Services.DTOs.Documents;

/// <summary>
/// Shared formatting for showing who uploaded a document. Keeps the
/// "Full Name &lt;email&gt;" / email fallback rule in one place so list,
/// details and RAG source displays stay consistent.
/// </summary>
public static class UploaderDisplay
{
    public static string Format(string? fullName, string? email)
    {
        var hasName = !string.IsNullOrWhiteSpace(fullName);
        var hasEmail = !string.IsNullOrWhiteSpace(email);

        if (hasName && hasEmail)
        {
            return $"{fullName} <{email}>";
        }

        if (hasName)
        {
            return fullName!;
        }

        if (hasEmail)
        {
            return email!;
        }

        return "-";
    }
}
