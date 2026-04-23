using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;

namespace Rodentia.Core.Services;

public sealed class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly RodentiaOptions _options;
    private readonly IMemoryCache _memoryCache;

    public LessonService(
        ILessonRepository lessonRepository,
        IOptions<RodentiaOptions> options,
        IMemoryCache memoryCache)
    {
        _lessonRepository = lessonRepository;
        _options = options.Value;
        _memoryCache = memoryCache;
    }

    public async Task<Result<IEnumerable<Lesson>>> GetScheduleAsync(Guid userId)
    {
        var lessons = await _lessonRepository.GetByUserIdAsync(userId);
        return Result<IEnumerable<Lesson>>.SuccessData(lessons);
    }

    public async Task<Result<CreateLessonModalDto>> GetCreateLessonModalDataAsync(
        Guid teacherId, CancellationToken cancellationToken = default)
    {
        var teacher = await _lessonRepository.GetUserByIdAsync(teacherId, cancellationToken);
        if (teacher is null)
            return Result<CreateLessonModalDto>.Failure("Користувача не знайдено.");
        if (teacher.Role != UserRole.Teacher)
            return Result<CreateLessonModalDto>.Failure("Лише викладач може створювати заняття.");

        var students = await GetCachedStudentsByTeacherIdAsync(teacherId, cancellationToken);

        var dto = new CreateLessonModalDto
        {
            Students = students
                .Select(x => new LessonStudentOptionDto
                {
                    StudentId = x.Id,
                    FullName = $"{x.FirstName} {x.LastName}".Trim()
                })
                .OrderBy(x => x.FullName)
                .ToList(),
            DefaultDate = DateTime.Today,
            DefaultDurationMinutes = Math.Max(60, _options.MinLessonDurationMinutes),
            DefaultStatus = LessonStatus.Scheduled
        };

        return Result<CreateLessonModalDto>.SuccessData(dto);
    }

    public async Task<Result> CreateLessonAsync(
        Guid teacherId, CreateLessonRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await _lessonRepository.GetUserByIdAsync(teacherId, cancellationToken);
        if (teacher is null) return Result.Failure("Користувача не знайдено.");
        if (teacher.Role != UserRole.Teacher) return Result.Failure("Лише викладач може створювати заняття.");
        if (request.StudentId == Guid.Empty) return Result.Failure("Оберіть дійсного учня.");
        if (request.LessonDate == default) return Result.Failure("Вкажіть коректну дату заняття.");

        if (request.DurationMinutes < _options.MinLessonDurationMinutes)
            return Result.Failure($"Тривалість не може бути меншою за {_options.MinLessonDurationMinutes} хвилин.");
        if (request.DurationMinutes > _options.MaxLessonDurationMinutes)
            return Result.Failure($"Тривалість не може перевищувати {_options.MaxLessonDurationMinutes} хвилин.");
        if (string.IsNullOrWhiteSpace(request.Subject))
            return Result.Failure("Вкажіть дисципліну.");

        var maxDate = DateTime.Today.AddDays(_options.ScheduleAheadDays);
        if (request.LessonDate.Date > maxDate)
            return Result.Failure($"Не можна планувати заняття більш ніж на {_options.ScheduleAheadDays} днів наперед.");

        var linkIsActive = await _lessonRepository.IsTeacherStudentLinkActiveAsync(
            teacherId, request.StudentId, cancellationToken);
        if (!linkIsActive) return Result.Failure("Цей учень не прикріплений до викладача.");

        var localScheduledAt = request.LessonDate.Date + request.StartTime;
        var scheduledAt = DateTime.SpecifyKind(localScheduledAt, DateTimeKind.Utc);

        var hasConflict = await _lessonRepository.HasConflictAsync(
            teacherId, request.StudentId, scheduledAt, request.DurationMinutes, null, cancellationToken);
        if (hasConflict) return Result.Failure("На цей час уже є інше заняття.");

        var lesson = new Lesson
        {
            TeacherId = teacherId,
            StudentId = request.StudentId,
            Subject = request.Subject.Trim(),
            Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim(),
            ScheduledAt = scheduledAt,
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            Status = request.Status,
            IsPaid = request.IsPaid,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Homework = string.IsNullOrWhiteSpace(request.Homework) ? null : request.Homework.Trim(),
            MaterialLinks = string.IsNullOrWhiteSpace(request.MaterialLinks) ? null : request.MaterialLinks.Trim(),
            ProgressNote = string.IsNullOrWhiteSpace(request.ProgressNote) ? null : request.ProgressNote.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _lessonRepository.AddAsync(lesson, cancellationToken);
        await _lessonRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<EditLessonModalDto>> GetEditLessonModalDataAsync(
        Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetLessonByIdAsync(lessonId, cancellationToken);
        if (lesson == null) return Result<EditLessonModalDto>.Failure("Заняття не знайдено.");
        if (lesson.TeacherId != teacherId) return Result<EditLessonModalDto>.Failure("Ви не маєте доступу до цього заняття.");

        var students = await GetCachedStudentsByTeacherIdAsync(teacherId, cancellationToken);

        var dto = new EditLessonModalDto
        {
            LessonId = lesson.Id,
            StudentId = lesson.StudentId,
            LessonDate = lesson.ScheduledAt.Date,
            StartTime = lesson.ScheduledAt.ToString("HH:mm"),
            DurationMinutes = lesson.DurationMinutes,
            Subject = lesson.Subject,
            Topic = lesson.Topic,
            Price = lesson.Price,
            Status = lesson.Status,
            IsPaid = lesson.IsPaid,
            Notes = lesson.Notes,
            Homework = lesson.Homework,
            MaterialLinks = lesson.MaterialLinks,
            ProgressNote = lesson.ProgressNote,
            Students = students.Select(x => new LessonStudentOptionDto
            {
                StudentId = x.Id,
                FullName = $"{x.FirstName} {x.LastName}".Trim()
            }).OrderBy(x => x.FullName).ToList()
        };

        return Result<EditLessonModalDto>.SuccessData(dto);
    }

    public async Task<Result<StudentPaymentOverviewDto>> GetStudentPaymentOverviewAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var lessons = (await _lessonRepository.GetByUserIdAsync(studentId, cancellationToken))
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.ScheduledAt)
            .ToList();

        var paidLessons = lessons
            .Where(x => x.IsPaid)
            .Select(x => new StudentPaymentLessonDto
            {
                LessonId = x.Id,
                ScheduledAt = x.ScheduledAt,
                Subject = x.Subject,
                Topic = x.Topic ?? string.Empty,
                Price = x.Price,
                IsPaid = x.IsPaid
            })
            .ToList();

        var debtLessons = lessons
            .Where(x => !x.IsPaid)
            .Select(x => new StudentPaymentLessonDto
            {
                LessonId = x.Id,
                ScheduledAt = x.ScheduledAt,
                Subject = x.Subject,
                Topic = x.Topic ?? string.Empty,
                Price = x.Price,
                IsPaid = x.IsPaid
            })
            .ToList();

        return Result<StudentPaymentOverviewDto>.SuccessData(new StudentPaymentOverviewDto
        {
            PaidTotal = paidLessons.Sum(x => x.Price),
            DebtTotal = debtLessons.Sum(x => x.Price),
            PaidLessons = paidLessons,
            DebtLessons = debtLessons
        });
    }

    public async Task<Result<TeacherLessonHistoryDto>> GetTeacherCompletedLessonsHistoryAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = (await _lessonRepository.GetByUserIdAsync(teacherId, cancellationToken))
            .Where(x => x.TeacherId == teacherId && x.Status == LessonStatus.Completed);

        if (fromDate.HasValue)
        {
            var fromBoundary = fromDate.Value.Date;
            query = query.Where(x => x.ScheduledAt >= fromBoundary);
        }

        if (toDate.HasValue)
        {
            var toBoundary = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.ScheduledAt <= toBoundary);
        }

        var lessons = query
            .OrderByDescending(x => x.ScheduledAt)
            .ToList();

        var historyItems = lessons
            .Select(x => new TeacherLessonHistoryItemDto
            {
                LessonId = x.Id,
                StudentId = x.StudentId,
                StudentName = string.IsNullOrWhiteSpace(x.DisplayName) ? "Невідомий учень" : x.DisplayName,
                ScheduledAt = x.ScheduledAt,
                Subject = x.Subject,
                Topic = x.Topic ?? string.Empty,
                Price = x.Price,
                IsPaid = x.IsPaid,
                Status = x.Status
            })
            .ToList();

        return Result<TeacherLessonHistoryDto>.SuccessData(new TeacherLessonHistoryDto
        {
            TotalCompletedLessons = historyItems.Count,
            TotalRevenue = historyItems.Sum(x => x.Price),
            Lessons = historyItems
        });
    }

    public async Task<Result> EditLessonAsync(
        Guid teacherId, EditLessonRequest request, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetLessonByIdAsync(request.LessonId, cancellationToken);
        if (lesson == null) return Result.Failure("Заняття не знайдено.");
        if (lesson.TeacherId != teacherId) return Result.Failure("Ви не маєте доступу до цього заняття.");

        if (request.DurationMinutes < _options.MinLessonDurationMinutes)
            return Result.Failure($"Тривалість має бути не меншою за {_options.MinLessonDurationMinutes} хвилин.");
        if (request.DurationMinutes > _options.MaxLessonDurationMinutes)
            return Result.Failure($"Тривалість не може перевищувати {_options.MaxLessonDurationMinutes} хвилин.");
        if (string.IsNullOrWhiteSpace(request.Subject))
            return Result.Failure("Предмет обов'язковий.");

        var localScheduledAt = request.LessonDate.Date.Add(request.StartTime);
        var scheduledAt = DateTime.SpecifyKind(localScheduledAt, DateTimeKind.Utc);

        var hasConflict = await _lessonRepository.HasConflictAsync(
            teacherId, request.StudentId, scheduledAt, request.DurationMinutes, lesson.Id, cancellationToken);
        if (hasConflict) return Result.Failure("На цей час уже є інше заняття.");

        lesson.StudentId = request.StudentId;
        lesson.ScheduledAt = scheduledAt;
        lesson.DurationMinutes = request.DurationMinutes;
        lesson.Subject = request.Subject.Trim();
        lesson.Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();
        lesson.Price = request.Price;
        lesson.Status = request.Status;
        lesson.IsPaid = request.IsPaid;
        lesson.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        lesson.Homework = string.IsNullOrWhiteSpace(request.Homework) ? null : request.Homework.Trim();
        lesson.MaterialLinks = string.IsNullOrWhiteSpace(request.MaterialLinks) ? null : request.MaterialLinks.Trim();
        lesson.ProgressNote = string.IsNullOrWhiteSpace(request.ProgressNote) ? null : request.ProgressNote.Trim();

        await _lessonRepository.UpdateAsync(lesson, cancellationToken);
        await _lessonRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeleteLessonAsync(
        Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetLessonByIdAsync(lessonId, cancellationToken);
        if (lesson == null) return Result.Failure("Заняття не знайдено.");
        if (lesson.TeacherId != teacherId) return Result.Failure("Ви не маєте доступу до цього заняття.");

        await _lessonRepository.DeleteAsync(lesson, cancellationToken);
        await _lessonRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task<List<User>> GetCachedStudentsByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var cacheKey = GetStudentsCacheKey(teacherId);
        if (_memoryCache.TryGetValue(cacheKey, out List<User> cachedStudents) && cachedStudents is not null)
            return cachedStudents;

        var students = (await _lessonRepository.GetActiveStudentsByTeacherIdAsync(teacherId, cancellationToken)).ToList();
        _memoryCache.Set(
            cacheKey,
            students,
            TimeSpan.FromMinutes(_options.StudentsCacheLifetimeMinutes));

        return students;
    }

    private static string GetStudentsCacheKey(Guid teacherId) => $"teacher:{teacherId}:students";
}