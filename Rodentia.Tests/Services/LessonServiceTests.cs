#nullable enable

using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models.Lessons;
using Rodentia.Core.Services;
using Xunit;

namespace Rodentia.Tests.Services;

public sealed class LessonServiceTests
{
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly LessonService _service;

    public LessonServiceTests()
    {
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _service = new LessonService(_lessonRepositoryMock.Object);
    }

    [Fact]
    public async Task GetScheduleAsync_ShouldReturnLessons_WhenRepositoryReturnsData()
    {
        var userId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new()
            {
                TeacherId = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                Subject = "Математика",
                ScheduledAt = DateTime.UtcNow,
                DurationMinutes = 60
            }
        };

        _lessonRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var result = await _service.GetScheduleAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data!);
        Assert.Equal("Математика", result.Data.First().Subject);
    }

    [Fact]
    public async Task GetCreateLessonModalDataAsync_ShouldReturnFailure_WhenTeacherNotFound()
    {
        var teacherId = Guid.NewGuid();

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var result = await _service.GetCreateLessonModalDataAsync(teacherId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetCreateLessonModalDataAsync_ShouldReturnFailure_WhenUserIsNotTeacher()
    {
        var studentUser = CreateUser(UserRole.Student);

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(studentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentUser);

        var result = await _service.GetCreateLessonModalDataAsync(studentUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Лише викладач може створювати заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetCreateLessonModalDataAsync_ShouldReturnStudents_WhenTeacherIsValid()
    {
        var teacher = CreateUser(UserRole.Teacher, "teach@test.com", "Олексій", "Тютор");
        var students = new List<User>
        {
            CreateUser(UserRole.Student, "student1@test.com", "Іван", "Петренко"),
            CreateUser(UserRole.Student, "student2@test.com", "Марія", "Коваленко")
        };

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        _lessonRepositoryMock
            .Setup(x => x.GetActiveStudentsByTeacherIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        var result = await _service.GetCreateLessonModalDataAsync(teacher.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Students.Count);
        Assert.Contains(result.Data.Students, x => x.FullName == "Іван Петренко");
        Assert.Contains(result.Data.Students, x => x.FullName == "Марія Коваленко");
        Assert.Equal(60, result.Data.DefaultDurationMinutes);
        Assert.Equal(LessonStatus.Scheduled, result.Data.DefaultStatus);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenTeacherNotFound()
    {
        var teacherId = Guid.NewGuid();
        var request = CreateValidRequest();

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var result = await _service.CreateLessonAsync(teacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenUserIsNotTeacher()
    {
        var student = CreateUser(UserRole.Student);
        var request = CreateValidRequest();

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.CreateLessonAsync(student.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Лише викладач може створювати заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenStudentIdIsEmpty()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = new CreateLessonRequest
        {
            StudentId = Guid.Empty,
            LessonDate = new DateTime(2026, 4, 3),
            StartTime = new TimeSpan(9, 0, 0),
            DurationMinutes = 60,
            Subject = "Математика",
            Topic = "Дроби",
            Price = 250m,
            Status = LessonStatus.Scheduled,
            Notes = "Підготувати вправи"
        };

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Потрібно вибрати учня.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenLessonDateIsDefault()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = new CreateLessonRequest
        {
            StudentId = Guid.NewGuid(),
            LessonDate = default,
            StartTime = new TimeSpan(9, 0, 0),
            DurationMinutes = 60,
            Subject = "Математика",
            Topic = "Дроби",
            Price = 250m,
            Status = LessonStatus.Scheduled,
            Notes = "Підготувати вправи"
        };

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Потрібно вибрати дату заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenDurationIsInvalid()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = new CreateLessonRequest
        {
            StudentId = Guid.NewGuid(),
            LessonDate = new DateTime(2026, 4, 3),
            StartTime = new TimeSpan(9, 0, 0),
            DurationMinutes = 0,
            Subject = "Математика",
            Topic = "Дроби",
            Price = 250m,
            Status = LessonStatus.Scheduled,
            Notes = "Підготувати вправи"
        };

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Тривалість має бути більшою за 0.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenSubjectIsEmpty()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = new CreateLessonRequest
        {
            StudentId = Guid.NewGuid(),
            LessonDate = new DateTime(2026, 4, 3),
            StartTime = new TimeSpan(9, 0, 0),
            DurationMinutes = 60,
            Subject = "   ",
            Topic = "Дроби",
            Price = 250m,
            Status = LessonStatus.Scheduled,
            Notes = "Підготувати вправи"
        };

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Предмет обов’язковий.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenStudentIsNotLinkedToTeacher()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = CreateValidRequest();

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        _lessonRepositoryMock
            .Setup(x => x.IsTeacherStudentLinkActiveAsync(
                teacher.Id,
                request.StudentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Цей учень не прив’язаний до викладача.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldReturnFailure_WhenScheduleHasConflict()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = CreateValidRequest();

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        _lessonRepositoryMock
            .Setup(x => x.IsTeacherStudentLinkActiveAsync(
                teacher.Id,
                request.StudentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _lessonRepositoryMock
            .Setup(x => x.HasConflictAsync(
                teacher.Id,
                request.StudentId,
                It.IsAny<DateTime>(),
                request.DurationMinutes,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("На цей час уже є інше заняття.", result.ErrorMessage);

        _lessonRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldCreateLesson_WhenRequestIsValid()
    {
        var teacher = CreateUser(UserRole.Teacher);
        var request = CreateValidRequest();

        Lesson? addedLesson = null;

        _lessonRepositoryMock
            .Setup(x => x.GetUserByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        _lessonRepositoryMock
            .Setup(x => x.IsTeacherStudentLinkActiveAsync(
                teacher.Id,
                request.StudentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _lessonRepositoryMock
            .Setup(x => x.HasConflictAsync(
                teacher.Id,
                request.StudentId,
                It.IsAny<DateTime>(),
                request.DurationMinutes,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _lessonRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()))
            .Callback<Lesson, CancellationToken>((lesson, _) => addedLesson = lesson)
            .Returns(Task.CompletedTask);

        _lessonRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateLessonAsync(teacher.Id, request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(addedLesson);
        Assert.Equal(teacher.Id, addedLesson!.TeacherId);
        Assert.Equal(request.StudentId, addedLesson.StudentId);
        Assert.Equal("Математика", addedLesson.Subject);
        Assert.Equal("Дроби", addedLesson.Topic);
        Assert.Equal(60, addedLesson.DurationMinutes);
        Assert.Equal(250m, addedLesson.Price);
        Assert.Equal(LessonStatus.Scheduled, addedLesson.Status);
        Assert.Equal("Підготувати вправи", addedLesson.Notes);

        _lessonRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _lessonRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static User CreateUser(
        UserRole role,
        string email = "user@test.com",
        string firstName = "Олексій",
        string lastName = "Тютор")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };
    }

    private static CreateLessonRequest CreateValidRequest()
    {
        return new CreateLessonRequest
        {
            StudentId = Guid.NewGuid(),
            LessonDate = new DateTime(2026, 4, 3),
            StartTime = new TimeSpan(9, 0, 0),
            DurationMinutes = 60,
            Subject = "Математика",
            Topic = "Дроби",
            Price = 250m,
            Status = LessonStatus.Scheduled,
            Notes = "Підготувати вправи"
        };
    }
}