using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using System.Security.Claims;

namespace Rodentia.Web.Controllers;

[Authorize] 
public class ScheduleController(ILessonService lessonService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await lessonService.GetScheduleAsync(userId);

        if (!result.Success)
        {
            return View("Error", result.ErrorMessage);
        }

        return View(result.Data); 
    }
}