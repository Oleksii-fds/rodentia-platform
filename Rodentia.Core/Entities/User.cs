#nullable enable

using Microsoft.AspNetCore.Identity;

namespace Rodentia.Core.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public UserRole Role { get; set; }

    public string? UniqueCode { get; set; }

    public string? AvatarPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}