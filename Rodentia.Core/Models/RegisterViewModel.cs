using System.ComponentModel.DataAnnotations;
using Rodentia.Core.Entities;

namespace Rodentia.Core.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Повне ім'я є обов'язковим")]
    [Display(Name = "Ім'я та Прізвище")]
    public string FullName { get; set; } = null!; 

    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Некоректний формат Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердіть пароль")] 
    [Compare("Password", ErrorMessage = "Паролі не збігаються")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "Оберіть вашу роль")]
    public UserRole Role { get; set; }
}