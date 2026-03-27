using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using System.Security.Claims;

namespace Rodentia.Web.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ITeacherService _teacherService;

    public TeacherController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> MyStudents()
    {
        var students = await _teacherService.GetMyStudentsAsync(CurrentUserId);
        return View(students);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return RedirectToAction(nameof(MyStudents));

        var result = await _teacherService.AddStudentAsync(CurrentUserId, identifier.Trim());
        
        if (result.IsSuccess)
            TempData["Success"] = "Учня додано до вашого списку.";
        else
            TempData["Error"] = result.ErrorMessage;
        
        return RedirectToAction(nameof(MyStudents));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(Guid studentId)
    {
        await _teacherService.RemoveStudentAsync(CurrentUserId, studentId);
        TempData["Success"] = "Зв'язок з учнем розірвано.";
        
        return RedirectToAction(nameof(MyStudents));
    }
}