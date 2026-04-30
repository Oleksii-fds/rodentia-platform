#nullable enable

using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;
using Rodentia.Core.Services;
using Xunit;

namespace Rodentia.Tests.Services;

public class LessonRescheduleRequestServiceTests
{
    private readonly Mock<ILessonRescheduleRequestRepository> _repositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly LessonRescheduleRequestService _service;

    public LessonRescheduleRequestServiceTests()
    {
        _repositoryMock = new Mock<ILessonRescheduleRequestRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.NotifyAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _service = new LessonRescheduleRequestService(_repositoryMock.Object, _notificationServiceMock.Object);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldFail_WhenLessonNotFound()
    {
        var actorId = Guid.NewGuid();
        var request = BuildCreateRequest(Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lesson)null!);

        var result = await _service.CreateRequestAsync(actorId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Заняття не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldFail_WhenActorNotParticipant()
    {
        var actorId = Guid.NewGuid();
        var lesson = BuildLesson();
        var request = BuildCreateRequest(lesson.Id);

        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);

        var result = await _service.CreateRequestAsync(actorId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви не маєте доступу до цього заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldFail_WhenConflictDetected()
    {
        var lesson = BuildLesson();
        var request = BuildCreateRequest(lesson.Id);

        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);
        _repositoryMock
            .Setup(x => x.HasPendingRequestForLessonAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(x => x.HasConflictAsync(
                lesson.TeacherId,
                lesson.StudentId,
                It.IsAny<DateTime>(),
                lesson.DurationMinutes,
                lesson.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateRequestAsync(lesson.TeacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("На запропонований час є конфлікт у розкладі.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldCreateRequest_WhenDataValid()
    {
        var lesson = BuildLesson();
        var request = BuildCreateRequest(lesson.Id);

        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);
        _repositoryMock
            .Setup(x => x.HasPendingRequestForLessonAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(x => x.HasConflictAsync(
                lesson.TeacherId,
                lesson.StudentId,
                It.IsAny<DateTime>(),
                lesson.DurationMinutes,
                lesson.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateRequestAsync(lesson.TeacherId, request);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<LessonRescheduleRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationType.LessonRescheduleRequested,
            lesson.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequestAsync_ShouldFail_WhenActorIsRequester()
    {
        var lesson = BuildLesson();
        var entity = BuildPendingRequest(lesson.Id, lesson.TeacherId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);

        var result = await _service.ApproveRequestAsync(lesson.TeacherId, entity.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви не можете підтвердити власний запит на перенесення.", result.ErrorMessage);
    }

    [Fact]
    public async Task ApproveRequestAsync_ShouldMoveLesson_WhenValid()
    {
        var lesson = BuildLesson();
        var entity = BuildPendingRequest(lesson.Id, lesson.StudentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);
        _repositoryMock
            .Setup(x => x.HasConflictAsync(
                lesson.TeacherId,
                lesson.StudentId,
                entity.ProposedScheduledAt,
                lesson.DurationMinutes,
                lesson.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.ApproveRequestAsync(lesson.TeacherId, entity.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(entity.ProposedScheduledAt, lesson.ScheduledAt);
        Assert.Equal(LessonRescheduleRequestStatus.Approved, entity.Status);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequestAsync_ShouldFail_WhenConflictDetected()
    {
        var lesson = BuildLesson();
        var entity = BuildPendingRequest(lesson.Id, lesson.StudentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);
        _repositoryMock
            .Setup(x => x.HasConflictAsync(
                lesson.TeacherId,
                lesson.StudentId,
                entity.ProposedScheduledAt,
                lesson.DurationMinutes,
                lesson.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.ApproveRequestAsync(lesson.TeacherId, entity.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("На запропонований час є конфлікт у розкладі.", result.ErrorMessage);
    }

    [Fact]
    public async Task RejectRequestAsync_ShouldReject_WhenValid()
    {
        var lesson = BuildLesson();
        var entity = BuildPendingRequest(lesson.Id, lesson.StudentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);

        var result = await _service.RejectRequestAsync(
            lesson.TeacherId,
            new RejectLessonRescheduleRequest { RequestId = entity.Id, Reason = "Не підходить час." });

        Assert.True(result.IsSuccess);
        Assert.Equal(LessonRescheduleRequestStatus.Rejected, entity.Status);
        Assert.Equal("Не підходить час.", entity.RejectionReason);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Lesson BuildLesson() => new()
    {
        Id = Guid.NewGuid(),
        TeacherId = Guid.NewGuid(),
        StudentId = Guid.NewGuid(),
        Status = LessonStatus.Scheduled,
        DurationMinutes = 60,
        ScheduledAt = DateTime.UtcNow.AddDays(1)
    };

    private static CreateLessonRescheduleRequest BuildCreateRequest(Guid lessonId) => new()
    {
        LessonId = lessonId,
        LessonDate = DateTime.UtcNow.Date.AddDays(2),
        StartTime = TimeSpan.FromHours(11),
        Reason = "Потрібно перенести через відрядження."
    };

    private static LessonRescheduleRequest BuildPendingRequest(Guid lessonId, Guid requestedByUserId) => new()
    {
        Id = Guid.NewGuid(),
        LessonId = lessonId,
        RequestedByUserId = requestedByUserId,
        ProposedScheduledAt = DateTime.UtcNow.AddDays(2),
        Reason = "Пропозиція нового часу.",
        Status = LessonRescheduleRequestStatus.Pending
    };
}
