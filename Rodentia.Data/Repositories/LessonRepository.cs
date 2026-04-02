#nullable enable

using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;

namespace Rodentia.Data.Repositories;

public sealed class LessonRepository : ILessonRepository
{
    private readonly RodentiaDbContext _dbContext;

    public LessonRepository(RodentiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Lesson>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (currentUser is null)
        {
            return [];
        }

        var lessons = await _dbContext.Lessons
            .Where(x => x.TeacherId == userId || x.StudentId == userId)
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync(cancellationToken);

        var relatedUserIds = lessons
            .Select(x => currentUser.Role == UserRole.Teacher ? x.StudentId : x.TeacherId)
            .Distinct()
            .ToList();

        var relatedUsers = await _dbContext.Users
            .AsNoTracking()
            .Where(x => relatedUserIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var lesson in lessons)
        {
            var relatedUserId = currentUser.Role == UserRole.Teacher
                ? lesson.StudentId
                : lesson.TeacherId;

            if (relatedUsers.TryGetValue(relatedUserId, out var relatedUser))
            {
                var lastInitial = string.IsNullOrWhiteSpace(relatedUser.LastName)
                    ? string.Empty
                    : $" {relatedUser.LastName[0]}.";

                lesson.DisplayName = $"{relatedUser.FirstName}{lastInitial}";
            }
            else
            {
                lesson.DisplayName = lesson.Subject;
            }
        }

        return lessons;
    }

    public async Task<User?> GetUserByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }
    public async Task<IEnumerable<User>> GetActiveStudentsByTeacherIdAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from link in _dbContext.TeacherStudentLinks
            join student in _dbContext.Users on link.StudentId equals student.Id
            where link.TeacherId == teacherId
                  && link.IsActive
                  && student.Role == UserRole.Student
            orderby student.FirstName, student.LastName
            select student
        ).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsTeacherStudentLinkActiveAsync(
        Guid teacherId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TeacherStudentLinks.AnyAsync(
            x => x.TeacherId == teacherId &&
                 x.StudentId == studentId &&
                 x.IsActive,
            cancellationToken);
    }

    public async Task<bool> HasConflictAsync(
        Guid teacherId,
        Guid studentId,
        DateTime scheduledAt,
        int durationMinutes,
        CancellationToken cancellationToken = default)
    {
        if (scheduledAt.Kind == DateTimeKind.Unspecified)
        {
            scheduledAt = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc);
        }
        else if (scheduledAt.Kind == DateTimeKind.Local)
        {
            scheduledAt = scheduledAt.ToUniversalTime();
        }

        var dayStart = DateTime.SpecifyKind(scheduledAt.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);
        var newEnd = scheduledAt.AddMinutes(durationMinutes);

        var lessons = await _dbContext.Lessons
            .Where(x =>
                x.Status != LessonStatus.Canceled &&
                x.ScheduledAt >= dayStart &&
                x.ScheduledAt < dayEnd &&
                (x.TeacherId == teacherId || x.StudentId == studentId))
            .ToListAsync(cancellationToken);

        return lessons.Any(x =>
        {
            var existingStart = x.ScheduledAt;
            var existingEnd = x.ScheduledAt.AddMinutes(x.DurationMinutes);

            return existingStart < newEnd && scheduledAt < existingEnd;
        });
    }

    public async Task AddAsync(
        Lesson lesson,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Lessons.AddAsync(lesson, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}