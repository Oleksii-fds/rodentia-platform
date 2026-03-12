using System.ComponentModel.DataAnnotations;

namespace Rodentia.Core.Entities;

public class User
{
    [Key] 
    public Guid Id { get; set; } = Guid.NewGuid(); 

    [Required] 
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public string? FirstName { get; set; } 

    public string? LastName { get; set; }

    [Required]
    public UserRole Role { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}