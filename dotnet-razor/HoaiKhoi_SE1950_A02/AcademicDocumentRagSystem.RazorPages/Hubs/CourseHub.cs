using Microsoft.AspNetCore.SignalR;

namespace AcademicDocumentRagSystem.RazorPages.Hubs
{
    /// <summary>
    /// Real-time channel for Course/Subject changes. Admins do not call this hub
    /// directly; instead the Courses page model broadcasts through
    /// <see cref="IHubContext{CourseHub}"/> after a successful service-layer CRUD
    /// operation. Lecturer clients subscribe and refresh their course list.
    /// </summary>
    public class CourseHub : Hub
    {
        // Client-side event names kept here so server and JS stay in sync.
        public const string CourseCreated = "CourseCreated";
        public const string CourseUpdated = "CourseUpdated";
        public const string CourseDeleted = "CourseDeleted";
        public const string CoursesChanged = "CoursesChanged";
    }
}
