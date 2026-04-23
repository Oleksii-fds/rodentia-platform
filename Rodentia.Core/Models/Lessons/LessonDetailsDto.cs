namespace Rodentia.Core.Models.Lessons;

public sealed class LessonDetailsDto
{
    public Guid LessonId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsPaid { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public string Homework { get; init; } = string.Empty;
    public string MaterialLinks { get; init; } = string.Empty;
    public string ProgressNote { get; init; } = string.Empty;
}
