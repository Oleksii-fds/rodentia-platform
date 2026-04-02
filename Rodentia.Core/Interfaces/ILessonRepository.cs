using Rodentia.Core.Entities;

namespace Rodentia.Core.Interfaces;

public interface ILessonRepository
{
    Task<IEnumerable<Lesson>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<User> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetActiveStudentsByTeacherIdAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<bool> IsTeacherStudentLinkActiveAsync(
        Guid teacherId,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<bool> HasConflictAsync(
        Guid teacherId,
        Guid studentId,
        DateTime scheduledAt,
        int durationMinutes,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Lesson lesson,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}