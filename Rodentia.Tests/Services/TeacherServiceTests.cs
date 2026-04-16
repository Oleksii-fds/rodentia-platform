using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Services;
using Rodentia.Tests.Helpers;
using Xunit;

namespace Rodentia.Tests.Services;

public class TeacherServiceTests
{
    private readonly Mock<ITeacherRepository> _repositoryMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly TeacherService _service;

    public TeacherServiceTests()
    {
        _repositoryMock = new Mock<ITeacherRepository>();
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var options = Options.Create(new RodentiaOptions { SearchMinLength = 2 });
        _service = new TeacherService(
            _repositoryMock.Object, _userManagerMock.Object, options);
    }

    private void SetupAsyncUserList(List<User> users)
    {
        _userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<User>(users));
    }


    [Fact]
    public async Task AddStudentAsync_ShouldReturnSuccess_WhenStudentFoundByEmail()
    {
        var teacherId = Guid.NewGuid();
        var student = new User { Id = Guid.NewGuid(), Email = "test@student.com", Role = UserRole.Student };

        SetupAsyncUserList(new List<User> { student });
        _repositoryMock.Setup(x => x.LinkExistsAsync(teacherId, student.Id)).ReturnsAsync(false);

        var result = await _service.AddStudentAsync(teacherId, "test@student.com");

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.AddLinkAsync(It.IsAny<TeacherStudentLink>()), Times.Once);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldReturnSuccess_WhenStudentFoundByUniqueCode()
    {
        var teacherId = Guid.NewGuid();
        var student = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@student.com",
            UniqueCode = "ROD-12345",
            Role = UserRole.Student
        };

        SetupAsyncUserList(new List<User> { student });
        _repositoryMock.Setup(x => x.LinkExistsAsync(teacherId, student.Id)).ReturnsAsync(false);

        var result = await _service.AddStudentAsync(teacherId, "ROD-12345");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldReturnFailure_WhenIdentifierTooShort()
    {
        var result = await _service.AddStudentAsync(Guid.NewGuid(), "a"); // < SearchMinLength=2

        Assert.False(result.IsSuccess);
        Assert.Contains("2", result.ErrorMessage);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldReturnFailure_WhenUserIsTeacher()
    {
        var teacherId = Guid.NewGuid();
        var otherTeacher = new User { Id = Guid.NewGuid(), Email = "teacher@test.com", Role = UserRole.Teacher };

        SetupAsyncUserList(new List<User> { otherTeacher });

        var result = await _service.AddStudentAsync(teacherId, "teacher@test.com");

        Assert.False(result.IsSuccess);
        Assert.Equal("Цей користувач не є учнем.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        SetupAsyncUserList(new List<User>());

        var result = await _service.AddStudentAsync(Guid.NewGuid(), "unknown@test.com");

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldReturnFailure_WhenLinkAlreadyExists()
    {
        var teacherId = Guid.NewGuid();
        var student = new User { Id = Guid.NewGuid(), Email = "already@student.com", Role = UserRole.Student };

        SetupAsyncUserList(new List<User> { student });
        _repositoryMock.Setup(x => x.LinkExistsAsync(teacherId, student.Id)).ReturnsAsync(true);

        var result = await _service.AddStudentAsync(teacherId, "already@student.com");

        Assert.False(result.IsSuccess);
        Assert.Equal("Учень вже є у вашому списку.", result.ErrorMessage);
    }


    [Fact]
    public async Task RemoveStudentAsync_ShouldCallRepository()
    {
        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var result = await _service.RemoveStudentAsync(teacherId, studentId);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.RemoveLinkAsync(teacherId, studentId), Times.Once);
    }


    [Fact]
    public async Task GetMyStudentsAsync_ShouldReturnAllLinkedStudents()
    {
        var teacherId = Guid.NewGuid();
        var students = new List<User>
        {
            new() { FirstName = "Leon",    Role = UserRole.Student },
            new() { FirstName = "Volodia", Role = UserRole.Student }
        };

        _repositoryMock
            .Setup(x => x.GetStudentsByTeacherIdAsync(teacherId))
            .ReturnsAsync(students);

        var result = await _service.GetMyStudentsAsync(teacherId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }


    [Fact]
    public async Task ConfirmPaymentAsync_ShouldReturnFailure_WhenLessonNotFound()
    {
        var teacherId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetLessonForTeacherAsync(teacherId, lessonId))
            .ReturnsAsync((Lesson)null);

        var result = await _service.ConfirmPaymentAsync(teacherId, lessonId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Заняття не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldReturnFailure_WhenAlreadyPaid()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId, IsPaid = true };

        _repositoryMock
            .Setup(x => x.GetLessonForTeacherAsync(teacherId, lesson.Id))
            .ReturnsAsync(lesson);

        var result = await _service.ConfirmPaymentAsync(teacherId, lesson.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Заняття вже відмічено як оплачене.", result.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldSetIsPaid_WhenSuccess()
    {
        var teacherId = Guid.NewGuid();
        var lesson = new Lesson { Id = Guid.NewGuid(), TeacherId = teacherId, IsPaid = false };

        _repositoryMock
            .Setup(x => x.GetLessonForTeacherAsync(teacherId, lesson.Id))
            .ReturnsAsync(lesson);

        var result = await _service.ConfirmPaymentAsync(teacherId, lesson.Id);

        Assert.True(result.IsSuccess);
        Assert.True(lesson.IsPaid);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task GetDebtAnalysisAsync_ShouldReturnEmptyDto_WhenNoUnpaidLessons()
    {
        var teacherId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetUnpaidLessonsByTeacherAsync(teacherId))
            .ReturnsAsync(new List<Lesson>());

        var result = await _service.GetDebtAnalysisAsync(teacherId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data!.TotalDebt);
        Assert.Empty(result.Data.Students);
    }

    [Fact]
    public async Task GetDebtAnalysisAsync_ShouldCalculateTotalDebt_WhenUnpaidLessonsExist()
    {
        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var student = new User
        {
            Id = studentId,
            FirstName = "Іван",
            LastName = "Тест"
        };

        var lessons = new List<Lesson>
        {
            new() { TeacherId = teacherId, StudentId = studentId, Price = 200, IsPaid = false, Subject = "Математика", ScheduledAt = DateTime.UtcNow },
            new() { TeacherId = teacherId, StudentId = studentId, Price = 150, IsPaid = false, Subject = "Фізика",     ScheduledAt = DateTime.UtcNow }
        };

        _repositoryMock
            .Setup(x => x.GetUnpaidLessonsByTeacherAsync(teacherId))
            .ReturnsAsync(lessons);

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<User>(new List<User> { student }));

        var result = await _service.GetDebtAnalysisAsync(teacherId);

        Assert.True(result.IsSuccess);
        Assert.Equal(350, result.Data!.TotalDebt);
        Assert.Single(result.Data.Students);
        Assert.Equal(350, result.Data.Students[0].TotalDebt);
        Assert.Equal(2, result.Data.Students[0].UnpaidLessonsCount);
    }
}