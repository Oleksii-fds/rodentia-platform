using Microsoft.AspNetCore.Identity;
using Rodentia.Core.Models;

namespace Rodentia.Core.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterViewModel model);
}