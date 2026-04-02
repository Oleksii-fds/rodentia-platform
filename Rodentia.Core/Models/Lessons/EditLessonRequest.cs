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
    public Entities.LessonStatus Status { get; set; }
    public string Notes { get; set; }
}