using Rodentia.Core.Entities;

namespace Rodentia.Core.Models.Lessons;

public sealed class EditLessonRequest
{
    public Guid LessonId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime LessonDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Topic { get; set; }
    public decimal Price { get; set; }
    public LessonStatus Status { get; set; }
    public bool IsPaid { get; set; }
    public string Notes { get; set; }
    public string Homework { get; set; }
    public string MaterialLinks { get; set; }
    public string ProgressNote { get; set; }
}