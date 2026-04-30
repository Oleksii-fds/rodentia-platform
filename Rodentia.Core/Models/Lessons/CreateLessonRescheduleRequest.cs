#nullable enable

namespace Rodentia.Core.Models.Lessons;

public sealed class CreateLessonRescheduleRequest
{
    public Guid LessonId { get; init; }

    public DateTime LessonDate { get; init; }

    public TimeSpan StartTime { get; init; }

    public string Reason { get; init; } = string.Empty;
}
