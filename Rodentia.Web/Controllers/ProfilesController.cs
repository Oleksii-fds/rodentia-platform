using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models.Profiles;
using Rodentia.Web.Filters;
using Rodentia.Web.Models.Profiles;

namespace Rodentia.Web.Controllers;

[Authorize]
public sealed class ProfilesController : BaseController
{
    private readonly IProfileService _profileService;
    private readonly IWebHostEnvironment _env;

    public ProfilesController(IProfileService profileService, IWebHostEnvironment env)
    {
        _profileService = profileService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> OwnModal(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetOwnProfileAsync(CurrentUserId, cancellationToken);

        if (!result.IsSuccess || result.Data is null)
            return NotFound();

        var vm = new OwnProfileModalViewModel
        {
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            Email = result.Data.Email,
            PhoneNumber = result.Data.PhoneNumber,
            RoleLabel = result.Data.RoleLabel,
            StudentCode = result.Data.StudentCode,
            AvatarPath = result.Data.AvatarPath
        };

        return PartialView("_OwnProfileModal", vm);
    }

    [HttpPost]
    [ValidateModelState]
    public async Task<IActionResult> UpdateOwnProfile(
        OwnProfileModalViewModel model,
        IFormFile avatarFile,
        CancellationToken cancellationToken)
    {
        string newAvatarPath = null;

        if (avatarFile is { Length: > 0 })
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(avatarFile.ContentType))
                return BadRequest(new { message = "Дозволені лише JPG, PNG, WEBP." });

            if (avatarFile.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Файл занадто великий (макс. 5 МБ)." });

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            var fileName = $"{CurrentUserId}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await avatarFile.CopyToAsync(stream, cancellationToken);

            newAvatarPath = $"/uploads/avatars/{fileName}";
        }

        var request = new UpdateOwnProfileRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            AvatarPath = newAvatarPath
        };

        var result = await _profileService.UpdateOwnProfileAsync(
            CurrentUserId, request, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage ?? "Не вдалося оновити профіль." });

        return Ok(new { message = "Профіль оновлено." });
    }
}