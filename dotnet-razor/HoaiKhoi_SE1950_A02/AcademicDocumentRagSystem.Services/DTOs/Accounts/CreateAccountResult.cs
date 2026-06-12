namespace AcademicDocumentRagSystem.Services.DTOs.Accounts
{
    /// <summary>
    /// Outcome of creating an account. The account is always persisted when this
    /// is returned; the email fields let the presentation layer warn the admin if
    /// the onboarding email could not be delivered (without failing the creation).
    /// </summary>
    public class CreateAccountResult
    {
        public int AccountId { get; set; }

        /// <summary>True when an onboarding email was supposed to be sent (Teacher accounts).</summary>
        public bool EmailAttempted { get; set; }

        /// <summary>True when the onboarding email was sent successfully.</summary>
        public bool EmailSent { get; set; }

        /// <summary>Failure reason when <see cref="EmailSent"/> is false.</summary>
        public string? EmailError { get; set; }
    }
}
