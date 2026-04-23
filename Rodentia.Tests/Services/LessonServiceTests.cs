using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;
using Rodentia.Core.Services;
using Xunit;

namespace Rodentia.Tests.Services;

public class LessonServiceTests
{
    private readonly Mock<ILessonRepository> _repoMock;
    private readonly LessonService _service;
    private readonly RodentiaOptions _options;
    private readonly IMemoryCache _memoryCache;

    public LessonServiceTests()
    {
        _repoMock = new Mock<ILessonRepository>();
        _options = new RodentiaOptions
        {
            MinLessonDurationMinutes = 15,
            MaxLessonDurationMinutes = 480,
            ScheduleAheadDays = 60,
            SearchMinLength = 2
        };
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _service = new LessonService(
            _repoMock.Object,
            Options.Create(_options),
            _memoryCache);
    }


    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var teacherId = Guid.NewGuid();
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        var result = await _service.CreateLessonAsync(teacherId, BuildRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenUserIsNotTeacher()
    {
        var teacher = BuildUser(UserRole.Student);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.CreateLessonAsync(teacher.Id, BuildRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Лише викладач може створювати заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenDurationTooShort()
    {
        var teacher = BuildUser(UserRole.Teacher);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var request = new CreateLessonRequest { StudentId = Guid.NewGuid(), LessonDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(10), DurationMinutes = 5, Subject = "Математика", Status = LessonStatus.Scheduled };

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Contains("15", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenDurationTooLong()
    {
        var teacher = BuildUser(UserRole.Teacher);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var request = new CreateLessonRequest { StudentId = Guid.NewGuid(), LessonDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(10), DurationMinutes = 999, Subject = "Математика", Status = LessonStatus.Scheduled };

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Contains("480", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenSubjectIsEmpty()
    {
        var teacher = BuildUser(UserRole.Teacher);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var request = new CreateLessonRequest { StudentId = Guid.NewGuid(), LessonDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(10), DurationMinutes = 60, Subject = "   ", Status = LessonStatus.Scheduled };

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Вкажіть дисципліну.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenDateTooFarAhead()
    {
        var teacher = BuildUser(UserRole.Teacher);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var request = new CreateLessonRequest { StudentId = Guid.NewGuid(), LessonDate = DateTime.Today.AddDays(999), StartTime = TimeSpan.FromHours(10), DurationMinutes = 60, Subject = "Математика", Status = LessonStatus.Scheduled };

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Contains("60", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenStudentNotLinked()
    {
        var teacher = BuildUser(UserRole.Teacher);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        _repoMock
            .Setup(x => x.IsTeacherStudentLinkActiveAsync(
                teacher.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateLessonAsync(teacher.Id, BuildRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Цей учень не прикріплений до викладача.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenConflictDetected()
    {
        var teacher = BuildUser(UserRole.Teacher);
        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        _repoMock
            .Setup(x => x.IsTeacherStudentLinkActiveAsync(
                teacher.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repoMock
            .Setup(x => x.HasConflictAsync(
                teacher.Id, It.IsAny<Guid>(), It.IsAny<DateTime>(),
                It.IsAny<int>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateLessonAsync(teacher.Id, BuildRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("На цей час уже є інше заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldSaveLesson_WhenAllValid()
    {
        var teacher = BuildUser(UserRole.Teacher);
        var studentId = Guid.NewGuid();

        _repoMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        _repoMock
            .Setup(x => x.IsTeacherStudentLinkActiveAsync(
                teacher.Id, studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repoMock
            .Setup(x => x.HasConflictAsync(
                teacher.Id, studentId, It.IsAny<DateTime>(),
                It.IsAny<int>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateLessonRequest { StudentId = studentId, LessonDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(10), DurationMinutes = 60, Subject = "Математика", Status = LessonStatus.Scheduled };
        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.True(result.IsSuccess);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task GetScheduleAsync_ShouldReturnLessons()
    {
        var userId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new() { TeacherId = userId, Subject = "Математика" },
            new() { TeacherId = userId, Subject = "Фізика" }
        };

        _repoMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var result = await _service.GetScheduleAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task GetStudentPaymentOverviewAsync_ShouldSplitPaidAndDebtLessons()
    {
        var studentId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new() { Id = Guid.NewGuid(), StudentId = studentId, Subject = "Математика", Price = 200, IsPaid = true, ScheduledAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = Guid.NewGuid(), StudentId = studentId, Subject = "Фізика", Price = 150, IsPaid = false, ScheduledAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), StudentId = studentId, Subject = "Хімія", Price = 100, IsPaid = true, ScheduledAt = DateTime.UtcNow }
        };

        _repoMock
            .Setup(x => x.GetByUserIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var result = await _service.GetStudentPaymentOverviewAsync(studentId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(300, result.Data!.PaidTotal);
        Assert.Equal(150, result.Data.DebtTotal);
        Assert.Equal(2, result.Data.PaidLessons.Count);
        Assert.Single(result.Data.DebtLessons);
    }

    [Fact]
    public async Task GetStudentPaymentOverviewAsync_ShouldIgnoreLessonsOfOtherStudents()
    {
        var studentId = Guid.NewGuid();
        var otherStudentId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new() { Id = Guid.NewGuid(), StudentId = studentId, Subject = "Математика", Price = 200, IsPaid = true, ScheduledAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), StudentId = otherStudentId, Subject = "Фізика", Price = 500, IsPaid = false, ScheduledAt = DateTime.UtcNow }
        };

        _repoMock
            .Setup(x => x.GetByUserIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var result = await _service.GetStudentPaymentOverviewAsync(studentId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(200, result.Data!.PaidTotal);
        Assert.Equal(0, result.Data.DebtTotal);
        Assert.Single(result.Data.PaidLessons);
        Assert.Empty(result.Data.DebtLessons);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldReturnFailure_WhenLessonNotFound()
    {
        var teacherId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _repoMock
            .Setup(x => x.GetLessonByIdAsync(lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lesson)null);

        var result = await _service.DeleteLessonAsync(teacherId, lessonId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Заняття не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldReturnFailure_WhenTeacherDoesNotOwnLesson()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = Guid.NewGuid() };

        _repoMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);

        var result = await _service.DeleteLessonAsync(teacherId, lesson.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви не маєте доступу до цього заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldDelete_WhenTeacherOwnsLesson()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId };

        _repoMock
            .Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);

        var result = await _service.DeleteLessonAsync(teacherId, lesson.Id);

        Assert.True(result.IsSuccess);
        _repoMock.Verify(x => x.DeleteAsync(lesson, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static User BuildUser(UserRole role) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Тест",
        LastName = "Юзер",
        Email = "test@test.com",
        Role = role
    };

    private static CreateLessonRequest BuildRequest() => new()
    {
        StudentId = Guid.NewGuid(),
        LessonDate = DateTime.Today.AddDays(1),
        StartTime = TimeSpan.FromHours(10),
        DurationMinutes = 60,
        Subject = "Математика",
        Status = LessonStatus.Scheduled
    };
}