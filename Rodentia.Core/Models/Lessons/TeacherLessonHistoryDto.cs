using Rodentia.Core.Entities;

namespace Rodentia.Core.Models.Lessons;

public sealed class TeacherLessonHistoryDto
{
    public int TotalCompletedLessons { get; init; }
    public decimal TotalRevenue { get; init; }
    public List<TeacherLessonHistoryItemDto> Lessons { get; init; } = [];
}

public sealed class TeacherLessonHistoryItemDto
{
    public Guid LessonId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsPaid { get; init; }
    public LessonStatus Status { get; init; }
}
