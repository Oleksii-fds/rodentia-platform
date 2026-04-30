#nullable enable

namespace Rodentia.Core.Models.Lessons;

public sealed class RejectLessonRescheduleRequest
{
    public Guid RequestId { get; init; }

    public string Reason { get; init; } = string.Empty;
}
