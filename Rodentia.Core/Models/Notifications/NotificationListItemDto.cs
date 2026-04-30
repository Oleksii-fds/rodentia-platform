#nullable enable

using Rodentia.Core.Entities;

namespace Rodentia.Core.Models.Notifications;

public sealed class NotificationListItemDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public NotificationType Type { get; init; }

    public DateTime CreatedAt { get; init; }

    public Guid? RelatedLessonId { get; init; }
}
