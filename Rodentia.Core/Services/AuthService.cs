using Microsoft.AspNetCore.Identity;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Core.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;

    public AuthService(UserManager<User> userManager)
    {
        _userManager = userManager;
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

        return await _userManager.CreateAsync(user, model.Password);
    }
}