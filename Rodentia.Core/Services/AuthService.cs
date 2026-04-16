using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using System.Security.Claims;

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

    public async Task<Result<IdentityResult>> RegisterAsync(RegisterViewModel model)
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
            UniqueCode = "ROD-" + Guid.NewGuid().ToString()[..5].ToUpper()
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role.ToString());

            await AddCustomClaimsAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Користувач {Email} зареєстрований як {Role}.", user.Email, model.Role);

            return result;
        }

        return Result<IdentityResult>.Failure(
            string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<SignInResult>> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null)
            await AddCustomClaimsAsync(user);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("Користувач {Email} успішно увійшов.", model.Email);
            return result;
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Акаунт користувача {Email} заблоковано.", model.Email);
            return Result<SignInResult>.Failure("Акаунт заблоковано. Спробуйте пізніше.");
        }

        return Result<SignInResult>.Failure("Невірний логін або пароль.");
    }

    public async Task<Result> SignOutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Користувач вийшов із системи.");
        return Result.Ok();
    }

    private async Task AddCustomClaimsAsync(User user)
    {
        var existingClaims = await _userManager.GetClaimsAsync(user);

        var firstNameClaim = existingClaims.FirstOrDefault(c => c.Type == "FirstName");
        if (firstNameClaim is not null)
            await _userManager.RemoveClaimAsync(user, firstNameClaim);
        await _userManager.AddClaimAsync(user, new Claim("FirstName", user.FirstName ?? ""));

        var lastNameClaim = existingClaims.FirstOrDefault(c => c.Type == "LastName");
        if (lastNameClaim is not null)
            await _userManager.RemoveClaimAsync(user, lastNameClaim);
        await _userManager.AddClaimAsync(user, new Claim("LastName", user.LastName ?? ""));

        var avatarClaim = existingClaims.FirstOrDefault(c => c.Type == "AvatarPath");
        if (avatarClaim is not null)
            await _userManager.RemoveClaimAsync(user, avatarClaim);
        if (!string.IsNullOrWhiteSpace(user.AvatarPath))
            await _userManager.AddClaimAsync(user, new Claim("AvatarPath", user.AvatarPath));
    }
}