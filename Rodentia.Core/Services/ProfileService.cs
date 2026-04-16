using Microsoft.Extensions.Logging;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Profiles;

namespace Rodentia.Core.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        IProfileRepository profileRepository,
        ILogger<ProfileService> logger)
    {
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<Result<OwnProfileDto>> GetOwnProfileAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _profileRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user is null)
            return Result<OwnProfileDto>.Failure("Користувача не знайдено.");

        var dto = new OwnProfileDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            RoleLabel = MapRoleLabel(user.Role),
            AvatarPath = user.AvatarPath,
            StudentCode = user.Role == UserRole.Student
                ? BuildStudentCode(user.Id)
                : null
        };

        return dto;
    }

    public async Task<Result> UpdateOwnProfileAsync(
        Guid currentUserId,
        UpdateOwnProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _profileRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user is null)
            return Result.Failure("Користувача не знайдено.");

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var email = request.Email.Trim();
        var phoneNumber = request.PhoneNumber?.Trim();

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure("Імʼя обовʼязкове.");
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure("Прізвище обовʼязкове.");
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure("Email обовʼязковий.");

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = phoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.AvatarPath))
            user.AvatarPath = request.AvatarPath;

        var updateResult = await _profileRepository.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Failure(JoinIdentityErrors(updateResult.Errors));

        var currentPassword = request.CurrentPassword?.Trim();
        var newPassword = request.NewPassword?.Trim();
        var hasOld = !string.IsNullOrWhiteSpace(currentPassword);
        var hasNew = !string.IsNullOrWhiteSpace(newPassword);

        if (hasOld && !hasNew) { hasOld = false; currentPassword = null; }

        if (!hasOld && hasNew)
            return Result.Failure("Для зміни пароля треба заповнити і старий, і новий пароль.");

        if (hasOld && hasNew)
        {
            var passwordResult = await _profileRepository.ChangePasswordAsync(
                user, currentPassword!, newPassword!);
            if (!passwordResult.Succeeded)
                return Result.Failure(JoinIdentityErrors(passwordResult.Errors));
        }

        _logger.LogInformation("User profile updated. UserId: {UserId}", user.Id);
        return Result.Ok();
    }

    private static string MapRoleLabel(UserRole role) => role switch
    {
        UserRole.Teacher => "Викладач",
        UserRole.Student => "Учень",
        _ => role.ToString()
    };

    private static string BuildStudentCode(Guid userId) =>
        $"#{userId.ToString("N")[..6].ToUpperInvariant()}";

    private static string JoinIdentityErrors(
        IEnumerable<Microsoft.AspNetCore.Identity.IdentityError> errors) =>
        string.Join(' ', errors.Select(x => x.Description)
            .Where(x => !string.IsNullOrWhiteSpace(x)));
}