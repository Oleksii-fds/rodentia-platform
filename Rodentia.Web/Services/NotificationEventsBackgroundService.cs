using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Data;
using Rodentia.Web.Hubs;

namespace Rodentia.Web.Services;

public sealed class NotificationEventsBackgroundService : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationEventsBackgroundService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationEventsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationEventsBackgroundService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification events background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<RodentiaDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                await ProcessUpcomingLessonsAsync(dbContext, notificationService, stoppingToken);
                await ProcessOverduePaymentsAsync(dbContext, notificationService, stoppingToken);
                await ProcessPendingRescheduleRequestsAsync(dbContext, notificationService, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification background scan failed");
            }

            try
            {
                await Task.Delay(ScanInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Notification events background service stopped");
    }

    private async Task ProcessUpcomingLessonsAsync(
        RodentiaDbContext dbContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowStart = now;
        var windowEnd = now.AddMinutes(30);

        var upcomingLessons = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Status == LessonStatus.Scheduled &&
                        x.ScheduledAt >= windowStart &&
                        x.ScheduledAt <= windowEnd)
            .ToListAsync(cancellationToken);

        foreach (var lesson in upcomingLessons)
        {
            var startsAtLocal = lesson.ScheduledAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            var title = "Нагадування про урок";
            var message = $"Урок з предмету \"{lesson.Subject}\" починається о {startsAtLocal}.";

            await TryNotifyOnceAsync(
                dbContext,
                notificationService,
                lesson.TeacherId,
                title,
                message,
                NotificationType.LessonStartingSoon,
                lesson.Id,
                now.AddHours(-2),
                cancellationToken);

            await TryNotifyOnceAsync(
                dbContext,
                notificationService,
                lesson.StudentId,
                title,
                message,
                NotificationType.LessonStartingSoon,
                lesson.Id,
                now.AddHours(-2),
                cancellationToken);
        }
    }

    private async Task ProcessOverduePaymentsAsync(
        RodentiaDbContext dbContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddHours(-24);

        var unpaidCompletedLessons = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Status == LessonStatus.Completed &&
                        !x.IsPaid &&
                        x.ScheduledAt <= threshold)
            .ToListAsync(cancellationToken);

        foreach (var lesson in unpaidCompletedLessons)
        {
            var lessonDate = lesson.ScheduledAt.ToLocalTime().ToString("dd.MM.yyyy");
            var title = "Неоплачений проведений урок";
            var message = $"Урок \"{lesson.Subject}\" від {lessonDate} позначено як проведений, але він досі не оплачений.";

            await TryNotifyOnceAsync(
                dbContext,
                notificationService,
                lesson.TeacherId,
                title,
                message,
                NotificationType.LessonPaymentOverdue,
                lesson.Id,
                now.AddHours(-24),
                cancellationToken);
        }
    }

    private async Task ProcessPendingRescheduleRequestsAsync(
        RodentiaDbContext dbContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var pendingFrom = now.AddMinutes(-15);

        var pendingRequests = await dbContext.LessonRescheduleRequests
            .AsNoTracking()
            .Where(x => x.Status == LessonRescheduleRequestStatus.Pending &&
                        x.CreatedAt <= pendingFrom)
            .ToListAsync(cancellationToken);

        if (pendingRequests.Count == 0)
        {
            return;
        }

        var lessonIds = pendingRequests.Select(x => x.LessonId).Distinct().ToList();
        var lessons = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => lessonIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var request in pendingRequests)
        {
            if (!lessons.TryGetValue(request.LessonId, out var lesson))
            {
                continue;
            }

            var reviewerId = request.RequestedByUserId == lesson.TeacherId
                ? lesson.StudentId
                : lesson.TeacherId;
            var proposedAt = request.ProposedScheduledAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            var title = "Очікує підтвердження перенесення";
            var message = $"Є непереглянутий запит на перенесення уроку \"{lesson.Subject}\" на {proposedAt}.";

            await TryNotifyOnceAsync(
                dbContext,
                notificationService,
                reviewerId,
                title,
                message,
                NotificationType.RescheduleRequestPendingReview,
                lesson.Id,
                now.AddHours(-12),
                cancellationToken);
        }
    }

    private async Task TryNotifyOnceAsync(
        RodentiaDbContext dbContext,
        INotificationService notificationService,
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid relatedLessonId,
        DateTime deduplicateFromUtc,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Notifications
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId &&
                           x.Type == type &&
                           x.RelatedLessonId == relatedLessonId &&
                           x.CreatedAt >= deduplicateFromUtc,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var notifyResult = await notificationService.NotifyAsync(
            userId,
            title,
            message,
            type,
            relatedLessonId,
            cancellationToken);

        if (!notifyResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to create notification for user {UserId}: {Error}",
                userId,
                notifyResult.ErrorMessage);
            return;
        }

        await _hubContext.Clients.Group(userId.ToString()).SendAsync(
            "notificationReceived",
            new
            {
                title,
                message,
                type = type.ToString(),
                createdAt = DateTime.UtcNow
            },
            cancellationToken);
    }
}
