using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Web.Filters;

namespace Rodentia.Web.Controllers;

public class AccountController : BaseController
{
    private readonly IAuthService _authService;
    


    public AccountController(IAuthService authService)
    {
        _authService = authService;


    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateModelState]

    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.RegisterAsync(model);

        if (result.IsSuccess)
        {
            Logger.LogInformation("Користувач {Email} успішно зареєстрований.", model.Email);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка реєстрації");

        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateModelState]

    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.LoginAsync(model);

        if (result.IsSuccess)
        {
            Logger.LogInformation("Користувач {Email} увійшов у систему.", model.Email);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Невірний логін або пароль.");
        
        return View(model);
    }

    [HttpPost]
    [Authorize]

    public async Task<IActionResult> Logout()
    {
        var result = await _authService.SignOutAsync();

        if (result.IsSuccess)
        {
            Logger.LogInformation("Користувач вийшов із системи.");
            return RedirectToAction("Login", "Account");
        }

        return RedirectToAction("Index", "Home");
    }
}