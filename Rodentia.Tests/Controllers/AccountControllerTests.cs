using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces; 
using Rodentia.Core.Models;    
using Rodentia.Web.Controllers;
using Xunit;

namespace Rodentia.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IAuthService> _authService;
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<ILogger<AccountController>> _logger;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _authService = new Mock<IAuthService>();
        _logger = new Mock<ILogger<AccountController>>();

        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _signInManager = new Mock<SignInManager<User>>(
            userManager.Object, 
            new Mock<IHttpContextAccessor>().Object, 
            new Mock<IUserClaimsPrincipalFactory<User>>().Object, 
            null!, null!, null!, null!);

        _controller = new AccountController(_authService.Object, _signInManager.Object, _logger.Object);
    }

    [Fact]
    public void Register_ReturnsViewResult()
    {
        var result = _controller.Register();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Register_Post_ValidModel_ReturnsRedirectToIndex()
    {
        
        _authService.Setup(x => x.RegisterAsync(It.IsAny<RegisterViewModel>()))
                    .ReturnsAsync(IdentityResult.Success);
        var model = new RegisterViewModel
        {
            FullName = "Марія Манько",
            Email = "maria@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Role = UserRole.Student
        };

        // Act
        var result = await _controller.Register(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Register_Post_InvalidModel_ReturnsViewWithModel()
    {
        _controller.ModelState.AddModelError("Email", "Required"); 
        var model = new RegisterViewModel(); 

        var result = await _controller.Register(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }
}