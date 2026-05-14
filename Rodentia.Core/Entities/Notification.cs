#nullable enable

namespace Rodentia.Core.Entities;

public sealed class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; } = NotificationType.General;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? RelatedLessonId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }
}

public enum NotificationType
{
    General = 0,
    LessonRescheduleRequested = 1,
    LessonRescheduleApproved = 2,
    LessonRescheduleRejected = 3,
    LessonStartingSoon = 4,
    LessonPaymentOverdue = 5,
    RescheduleRequestPendingReview = 6
}
