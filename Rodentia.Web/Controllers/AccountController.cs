using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;

        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.RegisterAsync(model);

        if (result.Success)
        {
            _logger.LogInformation("Користувач {Email} успішно зареєстрований.", model.Email);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка реєстрації");

        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.LoginAsync(model);

        if (result.Success)
        {
            _logger.LogInformation("Користувач {Email} увійшов у систему.", model.Email);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Невірний логін або пароль.");
        
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.SignOutAsync();

        if (result.Success)
        {
            _logger.LogInformation("Користувач вийшов із системи.");
            return RedirectToAction("Login", "Account");
        }

        return RedirectToAction("Index", "Home");
    }
}