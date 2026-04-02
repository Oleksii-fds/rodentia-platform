using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rodentia.Core.Entities;

namespace Rodentia.Web.Models.Lessons;

public sealed class EditLessonModalViewModel
{
    [Required]
    public Guid LessonId { get; set; }

    [Required(ErrorMessage = "Оберіть учня")]
    [Display(Name = "Учень")]
    public Guid StudentId { get; set; }

    public List<SelectListItem> StudentOptions { get; set; } = new();

    [Required(ErrorMessage = "Оберіть дату")]
    [Display(Name = "Дата заняття")]
    public DateTime LessonDate { get; set; }

    [Required(ErrorMessage = "Оберіть час")]
    [Display(Name = "Час початку")]
    public string StartTime { get; set; } = string.Empty;

    [Required]
    [Range(1, 480, ErrorMessage = "Тривалість має бути від 1 до 480 хвилин")]
    [Display(Name = "Тривалість (хв)")]
    public int DurationMinutes { get; set; }

    [Required(ErrorMessage = "Введіть предмет")]
    [Display(Name = "Предмет")]
    public string Subject { get; set; } = string.Empty;

    [Display(Name = "Тема (необов'язково)")]
    public string Topic { get; set; }

    [Range(0, 10000, ErrorMessage = "Некоректна ціна")]
    [Display(Name = "Ціна")]
    public decimal Price { get; set; }

    [Display(Name = "Статус")]
    public LessonStatus Status { get; set; }

    [Display(Name = "Нотатки")]
    public string Notes { get; set; }
}