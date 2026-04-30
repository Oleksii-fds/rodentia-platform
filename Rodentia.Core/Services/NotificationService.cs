#nullable enable

using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Notifications;

namespace Rodentia.Core.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result> NotifyAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type = NotificationType.General,
        Guid? relatedLessonId = null,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return Result.Failure("Отримувача сповіщення не знайдено.");
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Заголовок сповіщення обов'язковий.");
        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure("Текст сповіщення обов'язковий.");

        var notification = new Notification
        {
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            RelatedLessonId = relatedLessonId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<IEnumerable<NotificationListItemDto>>> GetUnreadAsync(
        Guid userId,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return Result<IEnumerable<NotificationListItemDto>>.Failure("Користувача не знайдено.");

        if (take < 1)
            take = 1;

        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId, take, cancellationToken);
        var items = notifications.Select(x => new NotificationListItemDto
        {
            Id = x.Id,
            Title = x.Title,
            Message = x.Message,
            Type = x.Type,
            CreatedAt = x.CreatedAt,
            RelatedLessonId = x.RelatedLessonId
        });

        return Result<IEnumerable<NotificationListItemDto>>.SuccessData(items);
    }

    public async Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return Result<int>.Failure("Користувача не знайдено.");

        var count = await _notificationRepository.CountUnreadByUserIdAsync(userId, cancellationToken);
        return Result<int>.SuccessData(count);
    }

    public async Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return Result.Failure("Користувача не знайдено.");
        if (notificationId == Guid.Empty)
            return Result.Failure("Некоректне сповіщення.");

        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null || notification.UserId != userId)
            return Result.Failure("Сповіщення не знайдено.");

        if (notification.IsRead)
            return Result.Ok();

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _notificationRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
