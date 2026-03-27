using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rodentia.Core.Entities;
using Rodentia.Core.Models;
using Rodentia.Core.Services;
using Microsoft.AspNetCore.Http;

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
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);
            
        _loggerMock = new Mock<ILogger<AuthService>>();
        _service = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenIdentitySucceeds()
    {
        var model = new RegisterViewModel 
        { 
            FullName = "Марія Манько", 
            Email = "manko@test.com", 
            Password = "Password123!",
            Role = UserRole.Teacher 
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.RegisterAsync(model);

        Assert.True(result.IsSuccess);
    }
}