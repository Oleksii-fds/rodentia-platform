using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Entities;
using Rodentia.Web.Models;

namespace Rodentia.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [HttpPost]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (ModelState.IsValid)
    {
        
        var fullNameParts = model.FullName.Trim().Split(' ');
        string firstName = fullNameParts.FirstOrDefault() ?? "";
        string lastName = fullNameParts.Length > 1 ? fullNameParts[1] : "";

        var user = new User 
        { 
            UserName = model.Email, 
            Email = model.Email, 
            FirstName = firstName, 
            LastName = lastName,   
            Role = model.Role 
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Користувач {Email} успішно зареєстрований.", user.Email);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
    return View(model);
}
}