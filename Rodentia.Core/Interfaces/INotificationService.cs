#nullable enable

using Rodentia.Core.Entities;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Notifications;

namespace Rodentia.Core.Interfaces;

public interface INotificationService
{
    Task<Result> NotifyAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type = NotificationType.General,
        Guid? relatedLessonId = null,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<NotificationListItemDto>>> GetUnreadAsync(
        Guid userId,
        int take = 10,
        CancellationToken cancellationToken = default);

    Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
}
