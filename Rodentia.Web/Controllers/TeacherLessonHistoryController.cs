using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;

namespace Rodentia.Web.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherLessonHistoryController(ILessonService lessonService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        var result = await lessonService.GetTeacherCompletedLessonsHistoryAsync(CurrentUserId, fromDate, toDate, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
            return View("Error", result.ErrorMessage);

        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(result.Data);
    }
}
