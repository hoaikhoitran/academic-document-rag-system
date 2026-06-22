namespace AcademicDocumentRagSystem.RazorPages.Infrastructure
{
    /// <summary>
    /// Session key names shared with the MVC app so the two presentation layers
    /// keep identical session semantics.
    /// </summary>
    public static class SessionKeys
    {
        public const string Email = "Email";
        public const string FullName = "FullName";
        public const string RoleName = "RoleName";
        public const string AccountId = "AccountId";
        public const string CourseId = "CourseId";
        public const string CourseCode = "CourseCode";
        public const string CourseName = "CourseName";
    }
}
