#nullable enable


using System.ComponentModel.DataAnnotations.Schema;

namespace Rodentia.Core.Entities;

public class Lesson
{

    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TeacherId { get; set; }
    
    public Guid StudentId { get; set; }
    
    public string Subject { get; set; } = "Математика";
    
    public string? Topic { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    
    public int DurationMinutes { get; set; } = 60;
    
    public decimal Price { get; set; }
    
    public LessonStatus Status { get; set; } = LessonStatus.Scheduled;
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string DisplayName { get; set; } = string.Empty;
}

public enum LessonStatus
{
    Scheduled,
    Completed,
    Canceled,
    Paid
}