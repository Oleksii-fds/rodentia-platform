using System.ComponentModel.DataAnnotations;

namespace Rodentia.Web.Models.Profiles;

public sealed class OwnProfileModalViewModel
{
    [Required(ErrorMessage = "Імʼя обовʼязкове")]
    [Display(Name = "Імʼя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Прізвище обовʼязкове")]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обовʼязковий")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Номер телефону")]
    public string PhoneNumber { get; set; }

    [Display(Name = "Старий пароль")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Display(Name = "Новий пароль")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    public string RoleLabel { get; set; } = string.Empty;
    public string StudentCode { get; set; }
    public bool ShowStudentCode => !string.IsNullOrWhiteSpace(StudentCode);
    public string AvatarPath { get; set; }

    [Display(Name = "Клас")]
    public string StudentClass { get; set; }
}