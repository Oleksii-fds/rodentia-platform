using Rodentia.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Rodentia.Core.Entities;
using Rodentia.Web.Controllers;
using Xunit;

namespace Rodentia.Tests.Controllers;

public class AccountControllerTests
{
    [Fact]
    public void Register_ReturnsViewResult()
    {
       
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var signInManager = new Mock<SignInManager<User>>(userManager.Object, new Mock<IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!, null!);
        var logger = new Mock<ILogger<AccountController>>();

        var controller = new AccountController(userManager.Object, signInManager.Object, logger.Object);

        var result = controller.Register();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Register_Post_ValidModel_ReturnsRedirectToIndex()
    {
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var signInManager = new Mock<SignInManager<User>>(userManager.Object, new Mock<IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!, null!);
        var logger = new Mock<ILogger<AccountController>>();

        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(userManager.Object, signInManager.Object, logger.Object);

        var model = new RegisterViewModel
        {
            FullName = "Марія Манько",
            Email = "maria@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Role = UserRole.Student
        };

        var result = await controller.Register(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Register_Post_InvalidModel_ReturnsViewWithModel()
    {
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var signInManager = new Mock<SignInManager<User>>(userManager.Object, new Mock<IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!, null!);
        var logger = new Mock<ILogger<AccountController>>();

        var controller = new AccountController(userManager.Object, signInManager.Object, logger.Object);
        controller.ModelState.AddModelError("Email", "Required"); 

        var model = new RegisterViewModel(); 
        var result = await controller.Register(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }
}