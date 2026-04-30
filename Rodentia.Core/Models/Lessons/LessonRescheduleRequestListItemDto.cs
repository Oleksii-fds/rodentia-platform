#nullable enable

namespace Rodentia.Core.Models.Lessons;

public sealed class LessonRescheduleRequestListItemDto
{
    public Guid RequestId { get; init; }

    public Guid RequestedByUserId { get; init; }

    public DateTime ProposedScheduledAt { get; init; }

    public string Reason { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public bool CanReview { get; init; }
}
