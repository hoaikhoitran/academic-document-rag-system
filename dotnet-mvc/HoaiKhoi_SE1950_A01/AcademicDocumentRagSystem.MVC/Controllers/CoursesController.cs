using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDocumentRagSystem.MVC.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetAllAsync();
        return View(courses);
    }

    public IActionResult Create()
    {
        return View(new CreateCourseDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _courseService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
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
            return View(dto);
        }

        try
        {
            await _courseService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}