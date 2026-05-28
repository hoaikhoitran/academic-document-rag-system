using AcademicDocumentRagSystem.Services.DTOs.Auth;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

public class AuthController : Controller
{
    private readonly IAccountService _accountService;

    public AuthController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public IActionResult Login()
    {
        return View(new LoginDto());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _accountService.LoginAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Login failed.");
            return View(dto);
        }

        HttpContext.Session.SetString("Email", result.Email);
        HttpContext.Session.SetString("FullName", result.FullName);
        HttpContext.Session.SetString("RoleName", result.RoleName);

        if (result.AccountId.HasValue)
        {
            HttpContext.Session.SetInt32("AccountId", result.AccountId.Value);
        }

        if (result.RoleName == "Admin")
        {
            return RedirectToAction("Index", "Courses");
        }

        if (result.RoleName == "Teacher")
        {
            return RedirectToAction("Index", "Courses");
        }

        return RedirectToAction("Index", "Courses");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}