using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using System.Security.Claims;

namespace Rodentia.Web.Controllers;

[Authorize] 
public class ScheduleController(ILessonService lessonService) : BaseController
{
    public async Task<IActionResult> Index()
    {
        

        var result = await lessonService.GetScheduleAsync(CurrentUserId);

        if (!result.IsSuccess)
        {
            return View("Error", result.ErrorMessage);
        }

        return View(result.Data); 
    }
}