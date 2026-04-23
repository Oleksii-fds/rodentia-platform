using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;

namespace Rodentia.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentPaymentsController(ILessonService lessonService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await lessonService.GetStudentPaymentOverviewAsync(CurrentUserId, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
            return View("Error", result.ErrorMessage);

        return View(result.Data);
    }
}
