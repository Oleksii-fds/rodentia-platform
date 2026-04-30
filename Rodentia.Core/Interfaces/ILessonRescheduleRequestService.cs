#nullable enable

using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;

namespace Rodentia.Core.Interfaces;

public interface ILessonRescheduleRequestService
{
    Task<Result<Guid>> CreateRequestAsync(
        Guid actorUserId,
        CreateLessonRescheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ApproveRequestAsync(
        Guid actorUserId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<Result> RejectRequestAsync(
        Guid actorUserId,
        RejectLessonRescheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<LessonRescheduleRequestListItemDto>>> GetPendingRequestsForLessonAsync(
        Guid actorUserId,
        Guid lessonId,
        CancellationToken cancellationToken = default);
}
