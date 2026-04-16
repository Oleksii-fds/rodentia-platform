using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models.Profiles;
using Rodentia.Core.Services;
using Xunit;

namespace Rodentia.Tests.Services;

public sealed class ProfileServiceTests
{
    private readonly Mock<IProfileRepository> _profileRepositoryMock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _profileRepositoryMock = new Mock<IProfileRepository>();
        _service = new ProfileService(
            _profileRepositoryMock.Object,
            NullLogger<ProfileService>.Instance);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        var result = await _service.GetOwnProfileAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ShouldReturnStudentCode_FromUniqueCodeField()
    {
        var user = CreateUser(UserRole.Student, uniqueCode: "ROD-AB123");
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetOwnProfileAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("ROD-AB123", result.Data!.StudentCode);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ShouldReturnNullStudentCode_WhenUserIsTeacher()
    {
        var user = CreateUser(UserRole.Teacher);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetOwnProfileAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Викладач", result.Data!.RoleLabel);
        Assert.Null(result.Data.StudentCode);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ShouldMapAllFields_Correctly()
    {
        var user = CreateUser(UserRole.Student);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetOwnProfileAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Data!.UserId);
        Assert.Equal("Іван", result.Data.FirstName);
        Assert.Equal("Петренко", result.Data.LastName);
        Assert.Equal("ivan@test.com", result.Data.Email);
        Assert.Equal("+380671112233", result.Data.PhoneNumber);
        Assert.Equal("Учень", result.Data.RoleLabel);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        var result = await _service.UpdateOwnProfileAsync(userId, CreateValidRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenFirstNameIsEmpty()
    {
        var user = CreateUser();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "   ",
            LastName = "Прізвище",
            Email = "e@test.com"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Імʼя обовʼязкове.", result.ErrorMessage);
        _profileRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenLastNameIsEmpty()
    {
        var user = CreateUser();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "   ",
            Email = "e@test.com"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Прізвище обовʼязкове.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenEmailIsEmpty()
    {
        var user = CreateUser();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "Прізвище",
            Email = "   "
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Email обовʼязковий.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldUpdateProfile_WithoutChangingPassword()
    {
        var user = CreateUser();
        User updatedUser = null;

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "  Михайло  ",
            LastName = "  Маринович  ",
            Email = "  mykhailo@test.com  ",
            PhoneNumber = "+380501234567"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Михайло", updatedUser!.FirstName);
        Assert.Equal("Маринович", updatedUser.LastName);
        Assert.Equal("mykhailo@test.com", updatedUser.Email);
        Assert.Equal("mykhailo@test.com", updatedUser.UserName);
        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenNewPasswordWithoutCurrentPassword()
    {
        var user = CreateUser();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "Прізвище",
            Email = "e@test.com",
            NewPassword = "NewPassword123!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Для зміни пароля треба заповнити і старий, і новий пароль.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldChangePassword_WhenBothPasswordsProvided()
    {
        var user = CreateUser();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _profileRepositoryMock
            .Setup(x => x.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "Прізвище",
            Email = "e@test.com",
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        });

        Assert.True(result.IsSuccess);
        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenChangePasswordFails()
    {
        var user = CreateUser();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _profileRepositoryMock
            .Setup(x => x.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

        var result = await _service.UpdateOwnProfileAsync(user.Id, new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "Прізвище",
            Email = "e@test.com",
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!"
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Incorrect password.", result.ErrorMessage);
    }

    // =========================================================================
    // ЮЗ КЕЙС 1: Викладач переглядає профіль учня — GetStudentProfileAsync
    // =========================================================================

    [Fact]
    public async Task GetStudentProfileAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var studentId = Guid.NewGuid();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        var result = await _service.GetStudentProfileAsync(studentId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetStudentProfileAsync_ShouldReturnFailure_WhenUserIsNotStudent()
    {
        var teacher = CreateUser(UserRole.Teacher);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.GetStudentProfileAsync(teacher.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Цей користувач не є учнем.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetStudentProfileAsync_ShouldReturnSuccess_WhenStudentExists()
    {
        var student = CreateUser(UserRole.Student);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.GetStudentProfileAsync(student.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetStudentProfileAsync_ShouldMapAllFields_Correctly()
    {
        var student = CreateUser(UserRole.Student, uniqueCode: "ROD-XY999", studentClass: "10-А");
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.GetStudentProfileAsync(student.Id);

        Assert.True(result.IsSuccess);
        var dto = result.Data!;
        Assert.Equal(student.Id, dto.UserId);
        Assert.Equal("Іван", dto.FirstName);
        Assert.Equal("Петренко", dto.LastName);
        Assert.Equal("ivan@test.com", dto.Email);
        Assert.Equal("+380671112233", dto.PhoneNumber);
        Assert.Equal("ROD-XY999", dto.StudentCode);
        Assert.Equal("10-А", dto.StudentClass);
        Assert.Equal("/uploads/av.jpg", dto.AvatarPath);
    }

    [Fact]
    public async Task GetStudentProfileAsync_ShouldReturnUniqueCode_FromDatabase()
    {
        var student = CreateUser(UserRole.Student, uniqueCode: "ROD-AB123");
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.GetStudentProfileAsync(student.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("ROD-AB123", result.Data!.StudentCode);
    }

    [Fact]
    public async Task GetStudentProfileAsync_ShouldReturnNullStudentClass_WhenNotSet()
    {
        var student = CreateUser(UserRole.Student, studentClass: null);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.GetStudentProfileAsync(student.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data!.StudentClass);
    }

    // =========================================================================
    // ЮЗ КЕЙС 2: Учень переглядає профіль викладача — GetTeacherProfileAsync
    // =========================================================================

    [Fact]
    public async Task GetTeacherProfileAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var teacherId = Guid.NewGuid();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        var result = await _service.GetTeacherProfileAsync(teacherId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetTeacherProfileAsync_ShouldReturnFailure_WhenUserIsNotTeacher()
    {
        var student = CreateUser(UserRole.Student);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.GetTeacherProfileAsync(student.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Цей користувач не є викладачем.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetTeacherProfileAsync_ShouldReturnSuccess_WhenTeacherExists()
    {
        var teacher = CreateTeacher();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.GetTeacherProfileAsync(teacher.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetTeacherProfileAsync_ShouldMapAllFields_Correctly()
    {
        var teacher = CreateTeacher();
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.GetTeacherProfileAsync(teacher.Id);

        Assert.True(result.IsSuccess);
        var dto = result.Data!;
        Assert.Equal(teacher.Id, dto.UserId);
        Assert.Equal("Олена", dto.FirstName);
        Assert.Equal("Коваленко", dto.LastName);
        Assert.Equal("olena@teacher.com", dto.Email);
        Assert.Equal("+380671119988", dto.PhoneNumber);
        Assert.Equal("/uploads/teach.jpg", dto.AvatarPath);
    }

    [Fact]
    public void GetTeacherProfileAsync_ShouldNotExposeStudentCode()
    {
        var prop = typeof(TeacherProfileDto).GetProperty("StudentCode");
        Assert.Null(prop);
    }

    [Fact]
    public async Task GetTeacherProfileAsync_ShouldReturnNullPhoneNumber_WhenNotSet()
    {
        var teacher = CreateTeacher(phoneNumber: null);
        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var result = await _service.GetTeacherProfileAsync(teacher.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data!.PhoneNumber);
    }



    private static User CreateUser(
        UserRole role = UserRole.Student,
        string uniqueCode = "ROD-11111",
        string studentClass = "10-А") => new()
        {
            Id = Guid.NewGuid(),
            FirstName = "Іван",
            LastName = "Петренко",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            PhoneNumber = "+380671112233",
            UniqueCode = uniqueCode,
            StudentClass = studentClass,
            AvatarPath = "/uploads/av.jpg",
            Role = role
        };

    private static User CreateTeacher(string phoneNumber = "+380671119988") => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Олена",
        LastName = "Коваленко",
        Email = "olena@teacher.com",
        UserName = "olena@teacher.com",
        PhoneNumber = phoneNumber,
        AvatarPath = "/uploads/teach.jpg",
        Role = UserRole.Teacher
    };

    private static UpdateOwnProfileRequest CreateValidRequest() => new()
    {
        FirstName = "НовеІмя",
        LastName = "НовеПрізвище",
        Email = "new@test.com"
    };
}