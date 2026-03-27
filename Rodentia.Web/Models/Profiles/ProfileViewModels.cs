using System.ComponentModel.DataAnnotations;

namespace Rodentia.Web.Models.Profiles;

public sealed class OwnProfileModalViewModel
{
    [Required(ErrorMessage = "Імʼя обовʼязкове.")]
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Прізвище обовʼязкове.")]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обовʼязковий.")]
    [EmailAddress(ErrorMessage = "Некоректний email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Номер телефону")]
    public string PhoneNumber { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Старий пароль")]
    public string CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Новий пароль")]
    public string NewPassword { get; set; }

    public string RoleLabel { get; set; } = string.Empty;

    public string StudentCode { get; set; }

    public bool ShowStudentCode => !string.IsNullOrWhiteSpace(StudentCode);
}