using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models.Profiles;
using Rodentia.Web.Models.Profiles;
using Rodentia.Web.Filters;

namespace Rodentia.Web.Controllers;

[Authorize]
public sealed class ProfilesController : BaseController
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
            CurrentUserId,
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
    [ValidateModelState]

    public async Task<IActionResult> UpdateOwnProfile(
        OwnProfileModalViewModel model,
        CancellationToken cancellationToken)
    {
        

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
            CurrentUserId,
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


}