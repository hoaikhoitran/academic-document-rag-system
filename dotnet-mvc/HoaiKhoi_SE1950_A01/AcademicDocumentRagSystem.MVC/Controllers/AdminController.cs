using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.MVC.ViewModels;
using AcademicDocumentRagSystem.MVC.ViewModels.Mock;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Admin")]
public class AdminController : Controller
{
    private readonly IAccountService _accountService;

    public AdminController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    private void SetSidebar()
    {
        DashboardLayoutHelper.SetAdminSidebar(this);
    }

    public IActionResult Dashboard()
    {
        SetSidebar();
        return View(MockDataProvider.GetAdminDashboardViewModel());
    }

    public IActionResult Research()
    {
        SetSidebar();
        return View(MockDataProvider.GetAdminResearchViewModel());
    }

    public IActionResult Benchmark()
    {
        SetSidebar();
        return View(MockDataProvider.GetAdminBenchmarkViewModel());
    }

    public async Task<IActionResult> Users(string? searchTerm, int? role, bool? status)
    {
        return RedirectToAction("Index", "Accounts", new { searchTerm, role, status });
    }

    public IActionResult Settings()
    {
        SetSidebar();
        return View();
    }
}
