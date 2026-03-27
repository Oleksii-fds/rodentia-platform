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
        // Arrange
        var userId = Guid.NewGuid();

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _service.GetOwnProfileAsync(userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ShouldReturnStudentProfile_WhenUserExists()
    {
        // Arrange
        var user = CreateUser(UserRole.Student);

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetOwnProfileAsync(user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data!.UserId);
        Assert.Equal("Іван", result.Data.FirstName);
        Assert.Equal("Петренко", result.Data.LastName);
        Assert.Equal("ivan@test.com", result.Data.Email);
        Assert.Equal("+380671112233", result.Data.PhoneNumber);
        Assert.Equal("Учень", result.Data.RoleLabel);
        Assert.Equal("#111111", result.Data.StudentCode);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ShouldReturnTeacherProfileWithoutStudentCode_WhenUserIsTeacher()
    {
        // Arrange
        var user = CreateUser(UserRole.Teacher);

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetOwnProfileAsync(user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Викладач", result.Data!.RoleLabel);
        Assert.Null(result.Data.StudentCode);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateValidRequest();

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _service.UpdateOwnProfileAsync(userId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenFirstNameIsEmpty()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "   ",
            LastName = "НовеПрізвище",
            Email = "new@test.com",
            PhoneNumber = "+380999999999"
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Імʼя обовʼязкове.", result.ErrorMessage);

        _profileRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenLastNameIsEmpty()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "   ",
            Email = "new@test.com",
            PhoneNumber = "+380999999999"
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Прізвище обовʼязкове.", result.ErrorMessage);

        _profileRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenEmailIsEmpty()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "   ",
            PhoneNumber = "+380999999999"
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email обовʼязковий.", result.ErrorMessage);

        _profileRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldUpdateProfileWithoutChangingPassword_WhenPasswordFieldsAreEmpty()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "  Михайло  ",
            LastName = "  Маринович  ",
            Email = "  mykhailo@test.com  ",
            PhoneNumber = "  +380501234567  ",
            CurrentPassword = "",
            NewPassword = ""
        };

        User updatedUser = null;

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        Assert.NotNull(updatedUser);
        Assert.Equal("Михайло", updatedUser!.FirstName);
        Assert.Equal("Маринович", updatedUser.LastName);
        Assert.Equal("mykhailo@test.com", updatedUser.Email);
        Assert.Equal("mykhailo@test.com", updatedUser.UserName);
        Assert.Equal("+380501234567", updatedUser.PhoneNumber);

        _profileRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Once);

        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldIgnoreCurrentPassword_WhenNewPasswordIsEmpty()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "new@test.com",
            PhoneNumber = "+380999999999",
            CurrentPassword = "autofilled-by-browser",
            NewPassword = "   "
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.True(result.IsSuccess);

        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenNewPasswordProvidedWithoutCurrentPassword()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "new@test.com",
            PhoneNumber = "+380999999999",
            CurrentPassword = "",
            NewPassword = "NewPassword123!"
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Для зміни пароля треба заповнити і старий, і новий пароль.", result.ErrorMessage);

        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenUserUpdateFails()
    {
        // Arrange
        var user = CreateUser();
        var request = CreateValidRequest();

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Email already taken." }));

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Email already taken.", result.ErrorMessage);

        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldChangePassword_WhenBothPasswordsProvided()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "new@test.com",
            PhoneNumber = "+380999999999",
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        _profileRepositoryMock
            .Setup(x => x.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.True(result.IsSuccess);

        _profileRepositoryMock.Verify(
            x => x.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_ShouldReturnFailure_WhenChangePasswordFails()
    {
        // Arrange
        var user = CreateUser();
        var request = new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "new@test.com",
            PhoneNumber = "+380999999999",
            CurrentPassword = "WrongOldPassword",
            NewPassword = "NewPassword123!"
        };

        _profileRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        _profileRepositoryMock
            .Setup(x => x.ChangePasswordAsync(user, "WrongOldPassword", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Incorrect password." }));

        // Act
        var result = await _service.UpdateOwnProfileAsync(user.Id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Incorrect password.", result.ErrorMessage);
    }

    private static User CreateUser(UserRole role = UserRole.Student)
    {
        return new User
        {
            Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            FirstName = "Іван",
            LastName = "Петренко",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            PhoneNumber = "+380671112233",
            Role = role,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    private static UpdateOwnProfileRequest CreateValidRequest()
    {
        return new UpdateOwnProfileRequest
        {
            FirstName = "НовеІмя",
            LastName = "НовеПрізвище",
            Email = "new@test.com",
            PhoneNumber = "+380999999999",
            CurrentPassword = "",
            NewPassword = ""
        };
    }
}