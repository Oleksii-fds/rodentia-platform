#nullable enable

namespace Rodentia.Core.Entities;

public sealed class LessonRescheduleRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LessonId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public DateTime ProposedScheduledAt { get; set; }

    public string Reason { get; set; } = string.Empty;

    public LessonRescheduleRequestStatus Status { get; set; } = LessonRescheduleRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string? RejectionReason { get; set; }
}

public enum LessonRescheduleRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
