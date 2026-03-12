public class Homework
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueAt { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}