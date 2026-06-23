using AcademicDocumentRagSystem.MVC.Filters;
using AcademicDocumentRagSystem.MVC.Hubs;
using AcademicDocumentRagSystem.MVC.ViewModels;
using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AcademicDocumentRagSystem.MVC.Controllers;

[SessionAuthorize("Admin")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IHubContext<CourseHub> _courseHub;

    public CoursesController(ICourseService courseService, IHubContext<CourseHub> courseHub)
    {
        _courseService = courseService;
        _courseHub = courseHub;
    }

    public async Task<IActionResult> Index()
    {
        DashboardLayoutHelper.SetAdminSidebar(this);
        var courses = await _courseService.GetAllAsync();
        return View(courses);
    }

    public IActionResult Create()
    {
        DashboardLayoutHelper.SetAdminSidebar(this);
        return View(new CreateCourseDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            DashboardLayoutHelper.SetAdminSidebar(this);
            return View(dto);
        }

        try
        {
            await _courseService.CreateAsync(dto);
            await _courseHub.Clients.All.SendAsync(CourseHub.CourseCreated, new { dto.Code, dto.Name });
            await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            DashboardLayoutHelper.SetAdminSidebar(this);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        DashboardLayoutHelper.SetAdminSidebar(this);
        var course = await _courseService.GetByIdAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        var dto = new UpdateCourseDto
        {
            CourseId = course.CourseId,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description,
            Status = course.Status
        };

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            DashboardLayoutHelper.SetAdminSidebar(this);
            return View(dto);
        }

        try
        {
            await _courseService.UpdateAsync(dto);
            await _courseHub.Clients.All.SendAsync(
                CourseHub.CourseUpdated, new { dto.CourseId, dto.Code, dto.Name });
            await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            DashboardLayoutHelper.SetAdminSidebar(this);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _courseService.DeleteAsync(id);
            await _courseHub.Clients.All.SendAsync(CourseHub.CourseDeleted, new { CourseId = id });
            await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
