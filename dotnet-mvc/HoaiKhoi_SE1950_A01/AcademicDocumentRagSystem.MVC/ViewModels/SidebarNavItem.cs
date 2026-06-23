namespace AcademicDocumentRagSystem.MVC.ViewModels;

public class SidebarNavItem
{
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public static class SidebarNav
{
    public static List<SidebarNavItem> Student { get; } = new()
    {
        new() { Controller = "Student", Action = "Chat", Label = "Hỏi đáp", Icon = "message" },
        new() { Controller = "Student", Action = "Library", Label = "Thư viện tài liệu", Icon = "book" },
        new() { Controller = "Student", Action = "Settings", Label = "Cài đặt", Icon = "settings" },
    };

    public static List<SidebarNavItem> Teacher { get; } = new()
    {
        new() { Controller = "Teacher", Action = "Courses", Label = "Môn học", Icon = "book" },
        new() { Controller = "Teacher", Action = "Library", Label = "Thư viện tài liệu", Icon = "file" },
        new() { Controller = "Teacher", Action = "Upload", Label = "Upload tài liệu", Icon = "upload" },
        new() { Controller = "Teacher", Action = "IndexStatus", Label = "Trạng thái index", Icon = "database" },
        new() { Controller = "Teacher", Action = "Chat", Label = "Chatbot", Icon = "message" },
        new() { Controller = "Teacher", Action = "Overview", Label = "Tổng quan", Icon = "dashboard" },
    };

    public static List<SidebarNavItem> Admin { get; } = new()
    {
        new() { Controller = "Admin", Action = "Dashboard", Label = "Dashboard", Icon = "dashboard" },
        new() { Controller = "Admin", Action = "Research", Label = "Module RBL", Icon = "beaker" },
        new() { Controller = "Admin", Action = "Benchmark", Label = "Benchmark RAGAS", Icon = "chart" },
        new() { Controller = "Accounts", Action = "Index", Label = "Người dùng", Icon = "users" },
        new() { Controller = "Courses", Action = "Index", Label = "Môn học", Icon = "book" },
        new() { Controller = "Documents", Action = "All", Label = "Tài liệu", Icon = "file" },
        new() { Controller = "Admin", Action = "Settings", Label = "Cấu hình hệ thống", Icon = "settings" },
    };
}
