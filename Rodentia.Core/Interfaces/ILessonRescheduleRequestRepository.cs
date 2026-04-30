#nullable enable

using Rodentia.Core.Entities;

namespace Rodentia.Core.Interfaces;

public interface ILessonRescheduleRequestRepository
{
    Task<Lesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<bool> HasPendingRequestForLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<bool> HasConflictAsync(
        Guid teacherId,
        Guid studentId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeLessonId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(LessonRescheduleRequest request, CancellationToken cancellationToken = default);

    Task<LessonRescheduleRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<IEnumerable<LessonRescheduleRequest>> GetPendingByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
