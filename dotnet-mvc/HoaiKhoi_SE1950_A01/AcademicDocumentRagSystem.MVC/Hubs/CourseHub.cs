using Microsoft.AspNetCore.SignalR;

namespace AcademicDocumentRagSystem.MVC.Hubs;

/// <summary>
/// Real-time channel for course changes. Admin CRUD broadcasts via
/// <see cref="IHubContext{CourseHub}"/>; teacher clients refresh their course list.
/// </summary>
public class CourseHub : Hub
{
    public const string CourseCreated = "CourseCreated";
    public const string CourseUpdated = "CourseUpdated";
    public const string CourseDeleted = "CourseDeleted";
    public const string CoursesChanged = "CoursesChanged";
}
