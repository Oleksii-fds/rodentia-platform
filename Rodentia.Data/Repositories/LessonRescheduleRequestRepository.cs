#nullable enable

using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;

namespace Rodentia.Data.Repositories;

public sealed class LessonRescheduleRequestRepository : ILessonRescheduleRequestRepository
{
    private readonly RodentiaDbContext _dbContext;

    public LessonRescheduleRequestRepository(RodentiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Lesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons.FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
    }

    public async Task<bool> HasPendingRequestForLessonAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonRescheduleRequests.AnyAsync(
            x => x.LessonId == lessonId && x.Status == LessonRescheduleRequestStatus.Pending,
            cancellationToken);
    }

    public async Task<bool> HasConflictAsync(
        Guid teacherId,
        Guid studentId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeLessonId = null,
        CancellationToken cancellationToken = default)
    {
        if (scheduledAt.Kind == DateTimeKind.Unspecified)
            scheduledAt = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc);
        else if (scheduledAt.Kind == DateTimeKind.Local)
            scheduledAt = scheduledAt.ToUniversalTime();

        var dayStart = DateTime.SpecifyKind(scheduledAt.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);
        var newEnd = scheduledAt.AddMinutes(durationMinutes);

        var lessons = await _dbContext.Lessons
            .Where(x =>
                x.Status != LessonStatus.Canceled &&
                x.ScheduledAt >= dayStart &&
                x.ScheduledAt < dayEnd &&
                (x.TeacherId == teacherId || x.StudentId == studentId) &&
                (excludeLessonId == null || x.Id != excludeLessonId))
            .ToListAsync(cancellationToken);

        return lessons.Any(x =>
        {
            var existingStart = x.ScheduledAt;
            var existingEnd = x.ScheduledAt.AddMinutes(x.DurationMinutes);
            return existingStart < newEnd && scheduledAt < existingEnd;
        });
    }

    public async Task AddAsync(LessonRescheduleRequest request, CancellationToken cancellationToken = default)
    {
        await _dbContext.LessonRescheduleRequests.AddAsync(request, cancellationToken);
    }

    public async Task<LessonRescheduleRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonRescheduleRequests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
    }

    public async Task<IEnumerable<LessonRescheduleRequest>> GetPendingByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonRescheduleRequests
            .Where(x => x.LessonId == lessonId && x.Status == LessonRescheduleRequestStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
