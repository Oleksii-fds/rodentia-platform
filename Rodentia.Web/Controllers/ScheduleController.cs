using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models.Lessons;
using Rodentia.Web.Models.Lessons;

namespace Rodentia.Web.Controllers;

[Authorize]
public class ScheduleController(ILessonService lessonService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? date)
    {
        var referenceDate = date ?? DateTime.Today;
        int diff = (7 + (referenceDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = referenceDate.AddDays(-1 * diff).Date;

        ViewBag.StartOfWeek = startOfWeek;
        ViewBag.SelectedDate = referenceDate.ToString("yyyy-MM-dd");

        var result = await lessonService.GetScheduleAsync(CurrentUserId);
        if (!result.IsSuccess)
            return View("Error", result.ErrorMessage);

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> CreateLessonModal(CancellationToken cancellationToken)
    {
        var result = await lessonService.GetCreateLessonModalDataAsync(CurrentUserId, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
            return BadRequest(result.ErrorMessage ?? "Не вдалося отримати дані створення заняття.");

        var vm = new CreateLessonModalViewModel
        {
            LessonDate = result.Data.DefaultDate,
            DurationMinutes = result.Data.DefaultDurationMinutes,
            Status = result.Data.DefaultStatus,
            StudentOptions = result.Data.Students
                .Select(x => new SelectListItem { Value = x.StudentId.ToString(), Text = x.FullName })
                .ToList()
        };

        return PartialView("_CreateLessonModal", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(
        CreateLessonModalViewModel model,
        CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParse(model.StartTime, out var startTime))
            ModelState.AddModelError(nameof(model.StartTime), "Некоректний формат часу.");

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Перевірте поля форми.",
                errors = ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
            });
        }

        var request = new CreateLessonRequest
        {
            StudentId = model.StudentId,
            LessonDate = model.LessonDate,
            StartTime = startTime,
            DurationMinutes = model.DurationMinutes,
            Subject = model.Subject,
            Topic = model.Topic,
            Price = model.Price,
            Status = model.Status,
            IsPaid = model.IsPaid,         
            Notes = model.Notes,
            Homework = model.Homework,        
            MaterialLinks = model.MaterialLinks,   
            ProgressNote = model.ProgressNote     
        };

        var result = await lessonService.CreateLessonAsync(CurrentUserId, request, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage ?? "Не вдалося створити заняття." });

        return Ok(new { message = "Заняття створено." });
    }

    [HttpGet]
    public async Task<IActionResult> EditLessonModal(Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await lessonService.GetEditLessonModalDataAsync(CurrentUserId, lessonId, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
            return BadRequest(result.ErrorMessage ?? "Не вдалося отримати дані для редагування заняття.");

        var vm = new EditLessonModalViewModel
        {
            LessonId = result.Data.LessonId,
            StudentId = result.Data.StudentId,
            LessonDate = result.Data.LessonDate,
            StartTime = result.Data.StartTime,
            DurationMinutes = result.Data.DurationMinutes,
            Subject = result.Data.Subject,
            Topic = result.Data.Topic,
            Price = result.Data.Price,
            Status = result.Data.Status,
            IsPaid = result.Data.IsPaid,          
            Notes = result.Data.Notes,
            Homework = result.Data.Homework,        
            MaterialLinks = result.Data.MaterialLinks,   
            ProgressNote = result.Data.ProgressNote,    
            StudentOptions = result.Data.Students
                .Select(x => new SelectListItem { Value = x.StudentId.ToString(), Text = x.FullName })
                .ToList()
        };

        return PartialView("_EditLessonModal", vm);
    }

    [HttpGet]
    public async Task<IActionResult> LessonDetailsModal(Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await lessonService.GetLessonDetailsAsync(CurrentUserId, lessonId, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
            return BadRequest(result.ErrorMessage ?? "Не вдалося отримати деталі заняття.");

        return PartialView("_LessonDetailsModal", result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(
        EditLessonModalViewModel model,
        CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParse(model.StartTime, out var startTime))
            ModelState.AddModelError(nameof(model.StartTime), "Некоректний формат часу.");

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Перевірте поля форми.",
                errors = ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
            });
        }

        var request = new EditLessonRequest
        {
            LessonId = model.LessonId,
            StudentId = model.StudentId,
            LessonDate = model.LessonDate,
            StartTime = startTime,
            DurationMinutes = model.DurationMinutes,
            Subject = model.Subject,
            Topic = model.Topic,
            Price = model.Price,
            Status = model.Status,
            IsPaid = model.IsPaid,          
            Notes = model.Notes,
            Homework = model.Homework,        
            MaterialLinks = model.MaterialLinks,   
            ProgressNote = model.ProgressNote     
        };

        var result = await lessonService.EditLessonAsync(CurrentUserId, request, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage ?? "Не вдалося оновити заняття." });

        return Ok(new { message = "Заняття успішно оновлено." });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await lessonService.DeleteLessonAsync(CurrentUserId, lessonId, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage ?? "Не вдалося видалити заняття." });

        return Ok(new { message = "Заняття успішно видалено." });
    }
}