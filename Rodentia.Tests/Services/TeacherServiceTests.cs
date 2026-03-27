using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Services;
using Rodentia.Core.Models;
using Rodentia.Tests.Helpers;

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
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _service = new TeacherService(_repositoryMock.Object, _userManagerMock.Object);
    }

    private void SetupAsyncUserList(List<User> users)
    {
        var asyncUsers = new TestAsyncEnumerable<User>(users);
        _userManagerMock.Setup(x => x.Users).Returns(asyncUsers);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldReturnSuccess_WhenStudentFoundByIdentifier()
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
        var teacherId = Guid.NewGuid();
        SetupAsyncUserList(new List<User>());

        var result = await _service.AddStudentAsync(teacherId, "unknown@test.com");

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
    public async Task GetMyStudentsAsync_ShouldReturnList()
    {
        var teacherId = Guid.NewGuid();
        var students = new List<User> 
        { 
            new User { FirstName = "Leon", Role = UserRole.Student },
            new User { FirstName = "Volodia", Role = UserRole.Student }
        };

        _repositoryMock.Setup(x => x.GetStudentsByTeacherIdAsync(teacherId))
            .ReturnsAsync(students);

        var result = await _service.GetMyStudentsAsync(teacherId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}