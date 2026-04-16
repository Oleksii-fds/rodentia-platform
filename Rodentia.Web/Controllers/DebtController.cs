using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;

namespace Rodentia.Web.Controllers;

[Authorize(Roles = "Teacher")]
public class DebtController : BaseController
{
    private readonly ITeacherService _teacherService;

    public DebtController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _teacherService.GetDebtAnalysisAsync(CurrentUserId);

        if (!result.IsSuccess || result.Data is null)
            return View("Error", result.ErrorMessage);

        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmPayment(Guid lessonId)
    {
        var result = await _teacherService.ConfirmPaymentAsync(CurrentUserId, lessonId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Оплату підтверджено." });
    }
}