#nullable enable

namespace Rodentia.Core.Models.Lessons;

public sealed class RecurringLessonCreationResultDto
{
    public int CreatedCount { get; init; }

    public List<RecurringLessonSkipDto> Skipped { get; init; } = [];
}

public sealed class RecurringLessonSkipDto
{
    public DateTime LessonDate { get; init; }

    public string Reason { get; init; } = string.Empty;
}
