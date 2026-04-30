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
    [Display(Name = "Тривалість (хв)")]
    public int DurationMinutes { get; set; } = 60;

    [Required(ErrorMessage = "Предмет обов'язковий.")]
    [Display(Name = "Предмет")]
    public string Subject { get; set; } = "Математика";

    [Display(Name = "Тема")]
    public string Topic { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Некоректна ціна.")]
    [Display(Name = "Ціна")]
    public decimal Price { get; set; }

    [Display(Name = "Статус заняття")]
    public LessonStatus Status { get; set; } = LessonStatus.Scheduled;

    [Display(Name = "Оплачено")]
    public bool IsPaid { get; set; } = false;

    [Display(Name = "Нотатки")]
    public string Notes { get; set; }

    [Display(Name = "Домашнє завдання")]
    public string Homework { get; set; }

    [Display(Name = "Матеріали та посилання")]
    public string MaterialLinks { get; set; }

    [Display(Name = "Успіхи та проблеми учня")]
    public string ProgressNote { get; set; }

    [Display(Name = "Періодичні заняття")]
    public bool IsRecurring { get; set; }

    [Display(Name = "До якої дати повторювати")]
    [DataType(DataType.Date)]
    public DateTime? RecurrenceEndDate { get; set; }

    [Range(1, 8, ErrorMessage = "Інтервал повторення має бути від 1 до 8 тижнів.")]
    [Display(Name = "Повторювати кожні (тижнів)")]
    public int RepeatEveryWeeks { get; set; } = 1;

    [Display(Name = "Дні тижня")]
    public List<DayOfWeek> RecurrenceDaysOfWeek { get; set; } = [];

    public List<SelectListItem> StudentOptions { get; set; } = [];
}