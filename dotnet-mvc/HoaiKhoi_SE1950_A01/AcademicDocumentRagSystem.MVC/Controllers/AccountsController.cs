using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.Services.DTOs.Accounts;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Admin")]
public class AccountsController : Controller
{
    private readonly IAccountService _accountService;
    private readonly ICourseService _courseService;

    public AccountsController(IAccountService accountService, ICourseService courseService)
    {
        _accountService = accountService;
        _courseService = courseService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? role, bool? status)
    {
        ViewBag.SearchTerm = searchTerm;
        ViewBag.Role = role;
        ViewBag.Status = status;

        var accounts = await _accountService.GetAllAsync(searchTerm, role, status);
        return View(accounts);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCoursesAsync();
        return View(new CreateAccountDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return View(dto);
        }

        try
        {
            await _accountService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCoursesAsync();
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var account = await _accountService.GetForEditAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        await LoadCoursesAsync();
        return View(account);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return View(dto);
        }

        try
        {
            await _accountService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCoursesAsync();
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _accountService.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCoursesAsync()
    {
        var courses = await _courseService.GetAllAsync();

        ViewBag.Courses = courses
            .Where(c => c.Status)
            .Select(c => new SelectListItem
            {
                Value = c.CourseId.ToString(),
                Text = $"{c.Code} - {c.Name}"
            })
            .ToList();
    }
}
