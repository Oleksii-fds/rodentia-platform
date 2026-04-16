namespace Rodentia.Core.Models;

public sealed class RodentiaOptions
{
    public const string SectionName = "Rodentia";

    public int PageSize { get; set; } = 20;

    public int SearchMinLength { get; set; } = 2;

    public int MaxLessonDurationMinutes { get; set; } = 480;

    public int MinLessonDurationMinutes { get; set; } = 15;

    public int ScheduleAheadDays { get; set; } = 60;
}