using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Core.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
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
            Role = model.Role,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Користувач {Email} успішно зареєстрований.", user.Email);
        }
        
        return result;

}

    public async Task<SignInResult> LoginAsync(LoginViewModel model)
{
    
        var result = await _signInManager.PasswordSignInAsync(
            model.Email, 
            model.Password, 
            model.RememberMe, 
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("Користувач {Email} успішно увійшов.", model.Email);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("Акаунт користувача {Email} заблоковано.", model.Email);
        }

        return result;
    
}

    

    public async Task SignOutAsync()
    {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Користувач вийшов із системи.");
            
    }

}