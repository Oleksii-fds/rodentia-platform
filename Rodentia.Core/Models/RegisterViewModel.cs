using System.ComponentModel.DataAnnotations;
using Rodentia.Core.Entities;

namespace Rodentia.Core.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ім'я")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть прізвище")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть Email")]
    [EmailAddress(ErrorMessage = "Невірний формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [MinLength(6, ErrorMessage = "Пароль мінімум 6 символів")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Student;

    public string FullName => $"{FirstName} {LastName}".Trim();
}