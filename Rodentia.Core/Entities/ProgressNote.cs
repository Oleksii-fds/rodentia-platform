public class ProgressNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public Guid TeacherId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? Score { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}