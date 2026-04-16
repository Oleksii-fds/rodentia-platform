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
        // UniqueCode тепер береться з БД, а не генерується з userId
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

        var request = new UpdateOwnProfileRequest
        {
            FirstName = "   ",
            LastName = "Прізвище",
            Email = "e@test.com"
        };

        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

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

        var request = new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "   ",
            Email = "e@test.com"
        };

        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

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

        var request = new UpdateOwnProfileRequest
        {
            FirstName = "Ім'я",
            LastName = "Прізвище",
            Email = "   "
        };

        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

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


    private static User CreateUser(UserRole role = UserRole.Student, string uniqueCode = "ROD-11111")
        => new()
        {
            Id = Guid.NewGuid(),
            FirstName = "Іван",
            LastName = "Петренко",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            PhoneNumber = "+380671112233",
            UniqueCode = uniqueCode,
            Role = role
        };

    private static UpdateOwnProfileRequest CreateValidRequest()
        => new()
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "new@test.com"
        };
}