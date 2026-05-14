using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Models;
using Rodentia.Core.Services;
using System.Security.Claims;
using Xunit;

namespace Rodentia.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<AuthService>>();
        _service = new AuthService(
            _userManagerMock.Object, _signInManagerMock.Object, _loggerMock.Object);

        _userManagerMock
            .Setup(x => x.GetClaimsAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<Claim>());

        _userManagerMock
            .Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        _signInManagerMock
            .Setup(x => x.SignInAsync(It.IsAny<User>(), false, null))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenIdentitySucceeds()
    {
        var model = new RegisterViewModel
        {
            FirstName = "Марія",
            LastName = "Манько",
            Email = "manko@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Role = UserRole.Teacher
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.RegisterAsync(model);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenIdentityFails()
    {
        var model = new RegisterViewModel
        {
            FirstName = "Марія",
            LastName = "Манько",
            Email = "manko@test.com",
            Password = "weak",
            ConfirmPassword = "weak",
            Role = UserRole.Teacher
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password too weak." }));

        var result = await _service.RegisterAsync(model);

        Assert.False(result.IsSuccess);
        Assert.Contains("Password too weak.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsValid()
    {
        var model = new LoginViewModel
        {
            Email = "manko@test.com",
            Password = "Password123!",
            RememberMe = false
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(model.Email))
            .ReturnsAsync(new User { Email = model.Email });

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(
                model.Email, model.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        var result = await _service.LoginAsync(model);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenCredentialsInvalid()
    {
        var model = new LoginViewModel
        {
            Email = "wrong@test.com",
            Password = "wrongpassword"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(model.Email))
            .ReturnsAsync((User)null);

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(
                model.Email, model.Password, false, false))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _service.LoginAsync(model);

        Assert.False(result.IsSuccess);
        Assert.Equal("Невірний логін або пароль.", result.ErrorMessage);
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnSuccess()
    {
        _signInManagerMock
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        var result = await _service.SignOutAsync();

        Assert.True(result.IsSuccess);
        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
    }
}