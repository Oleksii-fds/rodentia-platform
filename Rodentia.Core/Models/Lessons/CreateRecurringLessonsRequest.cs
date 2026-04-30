#nullable enable

using Rodentia.Core.Entities;

namespace Rodentia.Core.Models.Lessons;

public sealed class CreateRecurringLessonsRequest
{
    public Guid StudentId { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public TimeSpan StartTime { get; init; }

    public int RepeatEveryWeeks { get; init; } = 1;

    public List<DayOfWeek> DaysOfWeek { get; init; } = [];

    public int DurationMinutes { get; init; } = 60;

    public string Subject { get; init; } = "Математика";

    public string Topic { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public LessonStatus Status { get; init; } = LessonStatus.Scheduled;

    public bool IsPaid { get; init; }

    public string Notes { get; init; } = string.Empty;

    public string Homework { get; init; } = string.Empty;

    public string MaterialLinks { get; init; } = string.Empty;

    public string ProgressNote { get; init; } = string.Empty;
}
