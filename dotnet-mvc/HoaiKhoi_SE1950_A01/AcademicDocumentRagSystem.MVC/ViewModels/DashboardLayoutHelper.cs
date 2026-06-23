using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.ViewModels;

public static class DashboardLayoutHelper
{
    public static void SetAdminSidebar(Controller controller)
    {
        controller.ViewBag.SidebarAccent = "Admin";
        controller.ViewBag.SidebarRole = "Admin";
        controller.ViewBag.NavItems = SidebarNav.Admin;
    }
}
