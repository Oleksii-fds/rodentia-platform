using Microsoft.AspNetCore.Identity;
using Rodentia.Core.Models;

namespace Rodentia.Core.Interfaces;

public interface IAuthService
{
    Task<Result<IdentityResult>> RegisterAsync(RegisterViewModel model);
    Task<Result<SignInResult>> LoginAsync(LoginViewModel model);
    Task<Result> SignOutAsync();
}