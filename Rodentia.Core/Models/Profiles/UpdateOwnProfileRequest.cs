namespace Rodentia.Core.Models.Profiles;

public sealed class UpdateOwnProfileRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; }
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
    public string AvatarPath { get; init; }

    public string StudentClass { get; set; }
}