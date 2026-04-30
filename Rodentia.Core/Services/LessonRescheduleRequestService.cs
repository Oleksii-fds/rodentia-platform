#nullable enable

using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;

namespace Rodentia.Core.Services;

public sealed class LessonRescheduleRequestService : ILessonRescheduleRequestService
{
    private readonly ILessonRescheduleRequestRepository _repository;

    public LessonRescheduleRequestService(ILessonRescheduleRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> CreateRequestAsync(
        Guid actorUserId,
        CreateLessonRescheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var lesson = await _repository.GetLessonByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
            return Result<Guid>.Failure("Заняття не знайдено.");

        if (!IsParticipant(actorUserId, lesson))
            return Result<Guid>.Failure("Ви не маєте доступу до цього заняття.");

        if (lesson.Status != LessonStatus.Scheduled)
            return Result<Guid>.Failure("Запит на перенесення можна створити лише для запланованого заняття.");

        if (request.LessonDate == default)
            return Result<Guid>.Failure("Вкажіть коректну дату.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result<Guid>.Failure("Вкажіть причину перенесення.");

        var localScheduledAt = request.LessonDate.Date.Add(request.StartTime);
        var proposedScheduledAt = DateTime.SpecifyKind(localScheduledAt, DateTimeKind.Utc);

        if (proposedScheduledAt <= DateTime.UtcNow)
            return Result<Guid>.Failure("Нова дата заняття має бути в майбутньому.");

        var hasPending = await _repository.HasPendingRequestForLessonAsync(lesson.Id, cancellationToken);
        if (hasPending)
            return Result<Guid>.Failure("Для цього заняття вже є активний запит на перенесення.");

        var hasConflict = await _repository.HasConflictAsync(
            lesson.TeacherId,
            lesson.StudentId,
            proposedScheduledAt,
            lesson.DurationMinutes,
            lesson.Id,
            cancellationToken);
        if (hasConflict)
            return Result<Guid>.Failure("На запропонований час є конфлікт у розкладі.");

        var rescheduleRequest = new LessonRescheduleRequest
        {
            LessonId = lesson.Id,
            RequestedByUserId = actorUserId,
            ProposedScheduledAt = proposedScheduledAt,
            Reason = request.Reason.Trim(),
            Status = LessonRescheduleRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(rescheduleRequest, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.SuccessData(rescheduleRequest.Id);
    }

    public async Task<Result> ApproveRequestAsync(
        Guid actorUserId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(requestId, cancellationToken);
        if (request is null)
            return Result.Failure("Запит на перенесення не знайдено.");

        if (request.Status != LessonRescheduleRequestStatus.Pending)
            return Result.Failure("Запит уже оброблений.");

        var lesson = await _repository.GetLessonByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
            return Result.Failure("Заняття не знайдено.");

        if (!IsParticipant(actorUserId, lesson))
            return Result.Failure("Ви не маєте доступу до цього запиту.");

        if (request.RequestedByUserId == actorUserId)
            return Result.Failure("Ви не можете підтвердити власний запит на перенесення.");

        if (lesson.Status != LessonStatus.Scheduled)
            return Result.Failure("Перенесення можливе лише для запланованого заняття.");

        var hasConflict = await _repository.HasConflictAsync(
            lesson.TeacherId,
            lesson.StudentId,
            request.ProposedScheduledAt,
            lesson.DurationMinutes,
            lesson.Id,
            cancellationToken);
        if (hasConflict)
            return Result.Failure("На запропонований час є конфлікт у розкладі.");

        lesson.ScheduledAt = request.ProposedScheduledAt;
        request.Status = LessonRescheduleRequestStatus.Approved;
        request.ReviewedByUserId = actorUserId;
        request.ReviewedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> RejectRequestAsync(
        Guid actorUserId,
        RejectLessonRescheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure("Вкажіть причину відхилення.");

        var entity = await _repository.GetByIdAsync(request.RequestId, cancellationToken);
        if (entity is null)
            return Result.Failure("Запит на перенесення не знайдено.");

        if (entity.Status != LessonRescheduleRequestStatus.Pending)
            return Result.Failure("Запит уже оброблений.");

        var lesson = await _repository.GetLessonByIdAsync(entity.LessonId, cancellationToken);
        if (lesson is null)
            return Result.Failure("Заняття не знайдено.");

        if (!IsParticipant(actorUserId, lesson))
            return Result.Failure("Ви не маєте доступу до цього запиту.");

        if (entity.RequestedByUserId == actorUserId)
            return Result.Failure("Ви не можете відхилити власний запит на перенесення.");

        entity.Status = LessonRescheduleRequestStatus.Rejected;
        entity.RejectionReason = request.Reason.Trim();
        entity.ReviewedByUserId = actorUserId;
        entity.ReviewedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<IEnumerable<LessonRescheduleRequestListItemDto>>> GetPendingRequestsForLessonAsync(
        Guid actorUserId,
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await _repository.GetLessonByIdAsync(lessonId, cancellationToken);
        if (lesson is null)
            return Result<IEnumerable<LessonRescheduleRequestListItemDto>>.Failure("Заняття не знайдено.");

        if (!IsParticipant(actorUserId, lesson))
            return Result<IEnumerable<LessonRescheduleRequestListItemDto>>.Failure("Ви не маєте доступу до цього заняття.");

        var requests = await _repository.GetPendingByLessonIdAsync(lessonId, cancellationToken);
        var items = requests
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new LessonRescheduleRequestListItemDto
            {
                RequestId = x.Id,
                RequestedByUserId = x.RequestedByUserId,
                ProposedScheduledAt = x.ProposedScheduledAt,
                Reason = x.Reason,
                CreatedAt = x.CreatedAt,
                CanReview = x.RequestedByUserId != actorUserId
            })
            .ToList();

        return Result<IEnumerable<LessonRescheduleRequestListItemDto>>.SuccessData(items);
    }

    private static bool IsParticipant(Guid userId, Lesson lesson)
        => lesson.TeacherId == userId || lesson.StudentId == userId;
}
