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
        Assert.Equal("Оберіть дійсного учня.", result.ErrorMessage);
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
        Assert.Equal("Вкажіть коректну дату заняття.", result.ErrorMessage);
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
        Assert.Equal("Тривалість не може бути меншою за 0.", result.ErrorMessage);
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
        Assert.Equal("Вкажіть дисципліну.", result.ErrorMessage);
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
        Assert.Equal("Цей учень не прикріплений до викладача.", result.ErrorMessage);
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
                null,
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
                null,
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

    [Fact]
    public async Task GetEditLessonModalDataAsync_ShouldReturnFailure_WhenLessonNotFound()
    {
        var teacherId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(lessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Lesson)null!);

        var result = await _service.GetEditLessonModalDataAsync(teacherId, lessonId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Заняття не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetEditLessonModalDataAsync_ShouldReturnFailure_WhenTeacherNotOwner()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = Guid.NewGuid() };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);

        var result = await _service.GetEditLessonModalDataAsync(teacherId, lesson.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви не маєте доступу до цього заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetEditLessonModalDataAsync_ShouldReturnSuccess_WhenValid()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson 
        { 
            Id = Guid.NewGuid(), 
            TeacherId = teacherId, 
            ScheduledAt = DateTime.UtcNow, 
            DurationMinutes = 60,
            Subject = "Test Subject"
        };
        var students = new List<User> { CreateUser(UserRole.Student) };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(lesson.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);
        _lessonRepositoryMock.Setup(x => x.GetActiveStudentsByTeacherIdAsync(teacherId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(students);

        var result = await _service.GetEditLessonModalDataAsync(teacherId, lesson.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(lesson.Id, result.Data!.LessonId);
        Assert.Equal(lesson.Subject, result.Data.Subject);
        Assert.Single(result.Data.Students);
    }

    [Fact]
    public async Task EditLessonAsync_ShouldReturnFailure_WhenLessonNotFound()
    {
        var teacherId = Guid.NewGuid();
        var request = new EditLessonRequest { LessonId = Guid.NewGuid() };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Lesson)null!);

        var result = await _service.EditLessonAsync(teacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Заняття не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task EditLessonAsync_ShouldReturnFailure_WhenTeacherNotOwner()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = Guid.NewGuid() };
        var request = new EditLessonRequest { LessonId = lesson.Id };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);

        var result = await _service.EditLessonAsync(teacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви не маєте доступу до цього заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task EditLessonAsync_ShouldReturnFailure_WhenDurationIsInvalid()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId };
        var request = new EditLessonRequest { LessonId = lesson.Id, DurationMinutes = 0 };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);

        var result = await _service.EditLessonAsync(teacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Тривалість має бути більшою за 0.", result.ErrorMessage);
    }

    [Fact]
    public async Task EditLessonAsync_ShouldReturnFailure_WhenSubjectIsEmpty()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId };
        var request = new EditLessonRequest { LessonId = lesson.Id, DurationMinutes = 60, Subject = " " };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);

        var result = await _service.EditLessonAsync(teacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Предмет обов’язковий.", result.ErrorMessage);
    }

    [Fact]
    public async Task EditLessonAsync_ShouldReturnFailure_WhenHasConflict()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId };
        var request = new EditLessonRequest 
        { 
            LessonId = lesson.Id, 
            DurationMinutes = 60, 
            Subject = "Physics",
            LessonDate = DateTime.Today,
            StartTime = new TimeSpan(10, 0, 0)
        };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);
                             
        _lessonRepositoryMock.Setup(x => x.HasConflictAsync(teacherId, request.StudentId, It.IsAny<DateTime>(), request.DurationMinutes, lesson.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(true);

        var result = await _service.EditLessonAsync(teacherId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("На цей час уже є інше заняття.", result.ErrorMessage);
    }

    [Fact]
    public async Task EditLessonAsync_ShouldReturnSuccess_WhenValid()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId, Subject = "Old Subject" };
        var request = new EditLessonRequest 
        { 
            LessonId = lesson.Id, 
            StudentId = Guid.NewGuid(),
            Subject = "New Subject",
            DurationMinutes = 90,
            LessonDate = DateTime.Today,
            StartTime = new TimeSpan(14, 0, 0)
        };

        _lessonRepositoryMock.Setup(x => x.GetLessonByIdAsync(request.LessonId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(lesson);
                             
        _lessonRepositoryMock.Setup(x => x.HasConflictAsync(teacherId, request.StudentId, It.IsAny<DateTime>(), request.DurationMinutes, lesson.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(false);

        var result = await _service.EditLessonAsync(teacherId, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Subject", lesson.Subject);
        Assert.Equal(90, lesson.DurationMinutes);
        _lessonRepositoryMock.Verify(x => x.UpdateAsync(lesson, It.IsAny<CancellationToken>()), Times.Once);
        _lessonRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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