using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rodentia.Core.Entities;

namespace Rodentia.Web.Models.Lessons;

public sealed class CreateLessonModalViewModel
{
    [Display(Name = "Учень")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Потрібно вибрати дату.")]
    [Display(Name = "Дата")]
    [DataType(DataType.Date)]
    public DateTime LessonDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Потрібно вказати час.")]
    [Display(Name = "Час початку")]
    public string StartTime { get; set; } = "09:00";

    [Range(1, 600, ErrorMessage = "Тривалість має бути від 1 до 600 хв.")]
    [Display(Name = "Тривалість")]
    public int DurationMinutes { get; set; } = 60;

    [Required(ErrorMessage = "Предмет обов’язковий.")]
    [Display(Name = "Предмет")]
    public string Subject { get; set; } = "Математика";

    [Display(Name = "Тема")]
    public string Topic { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Некоректна ціна.")]
    [Display(Name = "Ціна")]
    public decimal Price { get; set; }

    [Display(Name = "Статус")]
    public LessonStatus Status { get; set; } = LessonStatus.Scheduled;

    [Display(Name = "Нотатки")]
    public string Notes { get; set; }

    public List<SelectListItem> StudentOptions { get; set; } = [];
}