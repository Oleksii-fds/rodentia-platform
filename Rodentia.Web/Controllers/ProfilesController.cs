using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models.Profiles;
using Rodentia.Web.Models.Profiles;

namespace Rodentia.Web.Controllers;

[Authorize]
public sealed class ProfilesController : Controller
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> OwnModal(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetOwnProfileAsync(
            GetCurrentUserId(),
            cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            return NotFound();
        }

        var vm = new OwnProfileModalViewModel
        {
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            Email = result.Data.Email,
            PhoneNumber = result.Data.PhoneNumber,
            RoleLabel = result.Data.RoleLabel,
            StudentCode = result.Data.StudentCode
        };

        return PartialView("_OwnProfileModal", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOwnProfile(
        OwnProfileModalViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Перевір поля форми.",
                errors = GetModelErrors()
            });
        }

        var request = new UpdateOwnProfileRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword
        };

        var result = await _profileService.UpdateOwnProfileAsync(
            GetCurrentUserId(),
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                message = result.ErrorMessage ?? "Не вдалося оновити профіль."
            });
        }

        return Ok(new
        {
            message = "Профіль оновлено."
        });
    }

    private Guid GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(rawUserId, out var userId))
        {
            throw new InvalidOperationException("Не вдалося визначити ID поточного користувача.");
        }

        return userId;
    }

    private IEnumerable<string> GetModelErrors()
    {
        return ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            .Where(x => !string.IsNullOrWhiteSpace(x));
    }
}