using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;

namespace Rodentia.Web.Controllers;

[Authorize]
public sealed class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Unread(int take = 10, CancellationToken cancellationToken = default)
    {
        var unreadResult = await _notificationService.GetUnreadAsync(CurrentUserId, take, cancellationToken);
        if (!unreadResult.IsSuccess || unreadResult.Data is null)
            return BadRequest(new { message = unreadResult.ErrorMessage ?? "Не вдалося отримати сповіщення." });

        var countResult = await _notificationService.GetUnreadCountAsync(CurrentUserId, cancellationToken);
        if (!countResult.IsSuccess)
            return BadRequest(new { message = countResult.ErrorMessage ?? "Не вдалося отримати кількість сповіщень." });

        return Ok(new
        {
            unreadCount = countResult.Data,
            items = unreadResult.Data.Select(x => new
            {
                id = x.Id,
                title = x.Title,
                message = x.Message,
                type = x.Type.ToString(),
                createdAt = x.CreatedAt,
                relatedLessonId = x.RelatedLessonId
            })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _notificationService.MarkAsReadAsync(CurrentUserId, id, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage ?? "Не вдалося позначити сповіщення прочитаним." });

        return Ok(new { message = "Сповіщення позначено як прочитане." });
    }
}
