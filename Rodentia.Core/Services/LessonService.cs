using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;

namespace Rodentia.Core.Services;

public sealed class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;

    public LessonService(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<Result<IEnumerable<Lesson>>> GetScheduleAsync(Guid userId)
    {
        var lessons = await _lessonRepository.GetByUserIdAsync(userId);
        return Result<IEnumerable<Lesson>>.SuccessData(lessons);
    }

    public async Task<Result<CreateLessonModalDto>> GetCreateLessonModalDataAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _lessonRepository.GetUserByIdAsync(teacherId, cancellationToken);
        if (teacher is null)
        {
            return Result<CreateLessonModalDto>.Failure("Користувача не знайдено.");
        }

        if (teacher.Role != UserRole.Teacher)
        {
            return Result<CreateLessonModalDto>.Failure("Лише викладач може створювати заняття.");
        }

        var students = await _lessonRepository.GetActiveStudentsByTeacherIdAsync(
            teacherId,
            cancellationToken);

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
            DefaultDurationMinutes = 60,
            DefaultStatus = LessonStatus.Scheduled
        };

        return Result<CreateLessonModalDto>.SuccessData(dto);
    }

    public async Task<Result> CreateLessonAsync(
        Guid teacherId,
        CreateLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _lessonRepository.GetUserByIdAsync(teacherId, cancellationToken);
        if (teacher is null)
        {
            return Result.Failure("Користувача не знайдено.");
        }

        if (teacher.Role != UserRole.Teacher)
        {
            return Result.Failure("Лише викладач може створювати заняття.");
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result.Failure("Оберіть дійсного учня.");
        }

        if (request.LessonDate == default)
        {
            return Result.Failure("Вкажіть коректну дату заняття.");
        }

        if (request.DurationMinutes <= 0)
        {
            return Result.Failure("Тривалість не може бути меншою за 0.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            return Result.Failure("Вкажіть дисципліну.");
        }

        var linkIsActive = await _lessonRepository.IsTeacherStudentLinkActiveAsync(
            teacherId,
            request.StudentId,
            cancellationToken);

        if (!linkIsActive)
        {
            return Result.Failure("Цей учень не прикріплений до викладача.");
        }

        var localScheduledAt = request.LessonDate.Date + request.StartTime;
        var scheduledAt = DateTime.SpecifyKind(localScheduledAt, DateTimeKind.Utc);

        var hasConflict = await _lessonRepository.HasConflictAsync(
            teacherId,
            request.StudentId,
            scheduledAt,
            request.DurationMinutes,
            null, 
            cancellationToken);

        if (hasConflict)
        {
            return Result.Failure("На цей час уже є інше заняття.");
        }

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
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _lessonRepository.AddAsync(lesson, cancellationToken);
        await _lessonRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<EditLessonModalDto>> GetEditLessonModalDataAsync(Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetLessonByIdAsync(lessonId, cancellationToken);
        
        if (lesson == null) return Result<EditLessonModalDto>.Failure("Заняття не знайдено.");
            
        if (lesson.TeacherId != teacherId) return Result<EditLessonModalDto>.Failure("Ви не маєте доступу до цього заняття.");

        var students = await _lessonRepository.GetActiveStudentsByTeacherIdAsync(teacherId, cancellationToken);

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
            Notes = lesson.Notes,
            Students = students.Select(x => new LessonStudentOptionDto
            {
                StudentId = x.Id,
                FullName = $"{x.FirstName} {x.LastName}".Trim()
            }).OrderBy(x => x.FullName).ToList()
        };

        return Result<EditLessonModalDto>.SuccessData(dto);
    }

    public async Task<Result> EditLessonAsync(Guid teacherId, EditLessonRequest request, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetLessonByIdAsync(request.LessonId, cancellationToken);
        if (lesson == null) return Result.Failure("Заняття не знайдено.");
        if (lesson.TeacherId != teacherId) return Result.Failure("Ви не маєте доступу до цього заняття.");

        if (request.DurationMinutes <= 0) return Result.Failure("Тривалість має бути більшою за 0.");
        if (string.IsNullOrWhiteSpace(request.Subject)) return Result.Failure("Предмет обов’язковий.");

        var localScheduledAt = request.LessonDate.Date.Add(request.StartTime);
        var scheduledAt = DateTime.SpecifyKind(localScheduledAt, DateTimeKind.Utc);

        var hasConflict = await _lessonRepository.HasConflictAsync(
            teacherId, 
            request.StudentId, 
            scheduledAt, 
            request.DurationMinutes, 
            lesson.Id, 
            cancellationToken);

        if (hasConflict) return Result.Failure("На цей час уже є інше заняття.");

        lesson.StudentId = request.StudentId;
        lesson.ScheduledAt = scheduledAt;
        lesson.DurationMinutes = request.DurationMinutes;
        lesson.Subject = request.Subject.Trim();
        lesson.Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();
        lesson.Price = request.Price;
        lesson.Status = request.Status;
        lesson.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _lessonRepository.UpdateAsync(lesson, cancellationToken);
        await _lessonRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    public async Task<Result> DeleteLessonAsync(Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default)
    {
    var lesson = await _lessonRepository.GetLessonByIdAsync(lessonId, cancellationToken);
    
    if (lesson == null) return Result.Failure("Заняття не знайдено.");
    if (lesson.TeacherId != teacherId) return Result.Failure("Ви не маєте доступу до цього заняття.");

    await _lessonRepository.DeleteAsync(lesson, cancellationToken);
    await _lessonRepository.SaveChangesAsync(cancellationToken);

    return Result.Ok();
    }
}