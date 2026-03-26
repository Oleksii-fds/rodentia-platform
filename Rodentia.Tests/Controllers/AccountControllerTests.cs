using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models; 
using Rodentia.Web.Controllers;
using Rodentia.Web.Models; 
using Xunit;

namespace Rodentia.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IAuthService> _authService;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<ILogger<AccountController>> _logger;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _authService = new Mock<IAuthService>();
        _logger = new Mock<ILogger<AccountController>>();

        var userStore = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManager = new Mock<SignInManager<User>>(
            _userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _controller = new AccountController(_authService.Object, _logger.Object);
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
        var model = new RegisterViewModel
        {
            FullName = "Марія Манько",
            Email = "maria@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Role = UserRole.Student
        };

        _authService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterViewModel>()))
            .ReturnsAsync(Result<IdentityResult>.SuccessData(IdentityResult.Success));

        _userManager
            .Setup(x => x.FindByEmailAsync(model.Email))
            .ReturnsAsync(new User { Email = model.Email, UserName = model.Email });

        _signInManager
            .Setup(x => x.SignInAsync(It.IsAny<User>(), false, null))
            .Returns(Task.CompletedTask);

        var result = await _controller.Register(model);

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

    [Fact]
    public async Task Register_Post_ValidModel_AutoSignsInUser()
    {
        var user = new User { Email = "maria@test.com", UserName = "maria@test.com" };
        var model = new RegisterViewModel
        {
            FullName = "Марія Манько",
            Email = "maria@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Role = UserRole.Student
        };

        _authService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterViewModel>()))
            .ReturnsAsync(Result<IdentityResult>.SuccessData(IdentityResult.Success));

        _userManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        _signInManager.Setup(x => x.SignInAsync(It.IsAny<User>(), false, null)).Returns(Task.CompletedTask);

        await _controller.Register(model);

        _authService.Verify(x => x.RegisterAsync(It.IsAny<RegisterViewModel>()), Times.Once());   
    }

    [Fact]
    public async Task Logout_ReturnsRedirectToRegister()
    {
        _authService.Setup(x => x.SignOutAsync()).ReturnsAsync(Result.Ok());

        var result = await _controller.Logout();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Logout_CallsSignOutAsync_Once()
    {
        _authService.Setup(x => x.SignOutAsync()).ReturnsAsync(Result.Ok());

        await _controller.Logout();

        _authService.Verify(x => x.SignOutAsync(), Times.Once());
    }

    [Fact]
    public void Logout_HasRequiredAttributes()
    {
        var method = typeof(AccountController).GetMethod("Logout");

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault());
        Assert.NotNull(method.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault());
        Assert.NotNull(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).FirstOrDefault());
    }
}